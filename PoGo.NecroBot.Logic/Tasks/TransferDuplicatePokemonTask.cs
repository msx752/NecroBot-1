﻿#region using directives

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using POGOProtos.Data;
using POGOProtos.Inventory;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class TransferDuplicatePokemonTask
    {
        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
            if (!session.LogicSettings.TransferDuplicatePokemon) return;
            if (session.LogicSettings.UseBulkTransferPokemon)
            {
                int buff = session.LogicSettings.BulkTransferStogareBuffer;
                //check for bag, if bag is nearly full, then process bulk transfer.
                var maxStorage = session.Profile.PlayerData.MaxPokemonStorage;
                var totalPokemon = session.Inventory.GetPokemons();
                var totalEggs = session.Inventory.GetEggs();
                if ((maxStorage - totalEggs.Count() - buff) > totalPokemon.Count()) return;
            }

            if (session.LogicSettings.AutoFavoritePokemon)
                await FavoritePokemonTask.Execute(session, cancellationToken);

            await EvolvePokemonTask.Execute(session, cancellationToken);

            var buddy = session.Profile.PlayerData.BuddyPokemon;
            var duplicatePokemons =
                await
                    session.Inventory.GetDuplicatePokemonToTransfer(
                        session.LogicSettings.PokemonsNotToTransfer,
                        session.LogicSettings.PokemonsToEvolve,
                        session.LogicSettings.KeepPokemonsThatCanEvolve,
                        session.LogicSettings.PrioritizeIvOverCp);
            
            if (buddy != null)
                duplicatePokemons = duplicatePokemons.Where(x => x.Id != buddy.Id);

            var orderedPokemon = duplicatePokemons.OrderBy(poke => poke.Cp);

            if (orderedPokemon.Count() == 0) return;

            if (session.LogicSettings.UseBulkTransferPokemon)
            {
                int page = orderedPokemon.Count() / session.LogicSettings.BulkTransferSize + 1;
                for (int i = 0; i < page; i++)
                {
                    TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                    var batchTransfer = orderedPokemon.Skip(i * session.LogicSettings.BulkTransferSize).Take(session.LogicSettings.BulkTransferSize);
                    var t = await session.Client.Inventory.TransferPokemons(batchTransfer.Select(x => x.Id).ToList());
                    if (t.Result == ReleasePokemonResponse.Types.Result.Success)
                    {
                        foreach (var duplicatePokemon in batchTransfer)
                        {
                            PrintPokemonInfo(session, duplicatePokemon);
                        }
                    }
                    else session.EventDispatcher.Send(new WarnEvent() { Message = session.Translation.GetTranslation(TranslationString.BulkTransferFailed, orderedPokemon.Count()) });
                }
            }
            else
                foreach (var duplicatePokemon in orderedPokemon)
                {
                    TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                    cancellationToken.ThrowIfCancellationRequested();

                    await session.Client.Inventory.TransferPokemon(duplicatePokemon.Id);

                    PrintPokemonInfo(session, duplicatePokemon);

                    // Padding the TransferEvent with player-choosen delay before instead of after.
                    // This is to remedy too quick transfers, often happening within a second of the
                    // previous action otherwise

                    await DelayingUtils.DelayAsync(session.LogicSettings.TransferActionDelay, 0, cancellationToken);
                }
        }

        public static void PrintPokemonInfo(ISession session, PokemonData duplicatePokemon)
        {
            var bestPokemonOfType = (session.LogicSettings.PrioritizeIvOverCp
                                        ? session.Inventory.GetHighestPokemonOfTypeByIv(duplicatePokemon)
                                        : session.Inventory.GetHighestPokemonOfTypeByCp(duplicatePokemon)) ??
                                    duplicatePokemon;

            var ev = new TransferPokemonEvent
            {
                Id = duplicatePokemon.Id,
                PokemonId = duplicatePokemon.PokemonId,
                Perfection = PokemonInfo.CalculatePokemonPerfection(duplicatePokemon),
                Cp = duplicatePokemon.Cp,
                BestCp = bestPokemonOfType.Cp,
                BestPerfection = PokemonInfo.CalculatePokemonPerfection(bestPokemonOfType),
                Candy = session.Inventory.GetCandyCount(duplicatePokemon.PokemonId)
            };

            if (session.Inventory.GetCandyFamily(duplicatePokemon.PokemonId) != null)
            {
                ev.FamilyId = session.Inventory.GetCandyFamily(duplicatePokemon.PokemonId).FamilyId;
            }

            session.EventDispatcher.Send(ev);
        }
    }
}