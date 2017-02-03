﻿#region using directives

using System.Linq;
using System.Threading.Tasks;
using PoGo.NecroBot.CLI.WebSocketHandler.GetCommands.Events;
using PoGo.NecroBot.CLI.WebSocketHandler.GetCommands.Helpers;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Inventory.Item;
using SuperSocket.WebSocket;

#endregion

namespace PoGo.NecroBot.CLI.WebSocketHandler.GetCommands.Tasks
{
    internal class GetEggListTask
    {
        // jjskuld - Ignore CS1998 warning for now.
        #pragma warning disable 1998
        public static async Task Execute(ISession session, WebSocketSession webSocketSession, string requestID)
        {
            // using (var blocker = new BlockableScope(session, BotActions.Eggs))
            {
                // if (!await blocker.WaitToRun()) return;

                var incubators = session.Inventory.GetEggIncubators()
                    .Where(x => x.UsesRemaining > 0 || x.ItemId == ItemId.ItemIncubatorBasicUnlimited)
                    .OrderByDescending(x => x.ItemId == ItemId.ItemIncubatorBasicUnlimited)
                    .ToList();

                var unusedEggs = session.Inventory.GetEggs()
                    .Where(x => string.IsNullOrEmpty(x.EggIncubatorId))
                    .OrderBy(x => x.EggKmWalkedTarget - x.EggKmWalkedStart)
                    .ToList();


                var list = new EggListWeb
                {
                    Incubators = incubators,
                    UnusedEggs = unusedEggs
                };
                webSocketSession.Send(EncodingHelper.Serialize(new EggListResponce(list, requestID)));
            }
        }
        #pragma warning restore 1998
    }
}