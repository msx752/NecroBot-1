﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using PoGo.Necrobot.Window.Model;
using PoGo.NecroBot.Logic.DataDumper;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Tasks;

namespace PoGo.Necrobot.Window.Controls
{
    public delegate void PokemonItemSelected(PokemonDataViewModel selected);

    /// <summary>
    /// Interaction logic for PokemonInventory.xaml
    /// </summary>
    public partial class PokemonInventory : UserControl
    {
        public ISession Session { get; set; }
        public event PokemonItemSelected OnPokemonItemSelected;

        //public static readonly DependencyProperty PokemonsProperty =
        // DependencyProperty.Register("Pokemons", typeof(string),
        //   typeof(List<PokemonData>), new PropertyMetadata(""));

        //public List<PokemonData> Pokemons
        //{
        //    get { return (List<PokemonData>)GetValue(PokemonsProperty); }
        //    set { SetValue(PokemonsProperty, value); }
        //}
        public PokemonInventory()
        {
            InitializeComponent();

            // gridData.ItemsSource = this.DataContext as List<PokemonData>;
        }

        private void gridData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // The bulk selection only works when 2 or more rows are selected.  This is to work around
            // issues with the checkbox and row selection when only one row is clicked.
            if (e.AddedItems.Count > 1)
            {
                foreach (PokemonDataViewModel pokemon in e.AddedItems)
                {
                    pokemon.IsSelected = true;
                }
            }

            var data = DataContext as PokemonListModel;
            var count = data.Pokemons.Count(x => x.IsSelected);
            //TODO : Thought it will better to use binding.
            btnTransferAll.Content = $"Transfer all ({count})";
            if (count > 1)
            {
                btnTransferAll.IsEnabled = true;
            }

            OnPokemonItemSelected?.Invoke(null);
        }

        private void btnTransfer_Click(object sender, RoutedEventArgs e)
        {
            var model = this.DataContext as PokemonListModel;

            ulong pokemonId = (ulong) ((Button) sender).CommandParameter;
            model.Transfer(pokemonId);
            var button = sender as Button;
            //button.Content = "Transfering...";
            //button.IsEnabled = false;

            Task.Run(async () =>
            {
                await TransferPokemonTask.Execute(
                    Session, Session.CancellationTokenSource.Token, new List<ulong> {pokemonId}
                );
            });
        }

        private void btnTransferAll_Click(object sender, RoutedEventArgs e)
        {
            var data = DataContext as PokemonListModel;
            var pokemonToTransfer = data.Pokemons
                .Where(x => x.IsSelected && !x.IsTransfering)
                .Select(x => x.Id)
                .ToList();
            data.Transfer(pokemonToTransfer);
            if (MessageBox.Show("Do you want to transfer all selected pokemon", "Bulk transfer", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Task.Run(async () =>
                {
                    await TransferPokemonTask.Execute(
                        Session, Session.CancellationTokenSource.Token, pokemonToTransfer
                    );
                });
            }
        }

        private void btnEvolve_Click(object sender, RoutedEventArgs e)
        {
            var model = this.DataContext as PokemonListModel;

            ulong pokemonId = (ulong) ((Button) sender).CommandParameter;
            model.Evolve(pokemonId);

            Task.Run(async () => { await EvolveSpecificPokemonTask.Execute(Session, pokemonId); });
        }

        private void btnFavorite_Click(object sender, RoutedEventArgs e)
        {
            var model = this.DataContext as PokemonListModel;

            ulong pokemonId = (ulong) ((Button) sender).CommandParameter;
            bool state = model.Favorite(pokemonId);

            Task.Run(async () => { await FavoritePokemonTask.Execute(Session, pokemonId, !state); });
        }

        private void Select_Checked(object sender, RoutedEventArgs e)
        {
            ulong pokemonId = (ulong)((CheckBox)sender).CommandParameter;

            var data = DataContext as PokemonListModel;
            var count = data.Pokemons.Count(x => x.IsSelected);
            //TODO : Thought it will better to use binding.
            btnTransferAll.Content = $"Transfer all ({count})";
            if (count > 1)
            {
                btnTransferAll.IsEnabled = true;
            }

            OnPokemonItemSelected?.Invoke(null);
        }

        private void btnPowerup_Click(object sender, RoutedEventArgs e)
        {
            var model = this.DataContext as PokemonListModel;

            ulong pokemonId = (ulong) ((Button) sender).CommandParameter;
            model.Powerup(pokemonId);

            Task.Run(async () => { await UpgradeSinglePokemonTask.Execute(Session, pokemonId, false); });
        }

        private void btnPokedexView_Click(object sender, RoutedEventArgs e)
        {
            PokedexWindow dexWindow = new PokedexWindow(this.Session);
            dexWindow.Show();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog()
            {
                Filter = "Excel (.xlsx)|*.xlsx",
            };

            if (dlg.ShowDialog() == true)
            {
                Dumper.SaveAsExcel(this.Session, dlg.FileName).ContinueWith((t) => { Process.Start(dlg.FileName); });
            }
        }

        private void btnMaxPowerUp_Click(object sender, RoutedEventArgs e)
        {
            var model = this.DataContext as PokemonListModel;

            ulong pokemonId = (ulong) ((Button) sender).CommandParameter;
            model.Powerup(pokemonId);

            Task.Run(async () => { await UpgradeSinglePokemonTask.Execute(Session, pokemonId, true); });
        }

        private void mnuBuddy_Click(object sender, RoutedEventArgs e)
        {
            //Get the clicked MenuItem
            var menuItem = (MenuItem) sender;

            //Get the ContextMenu to which the menuItem belongs
            var contextMenu = (ContextMenu) menuItem.Parent;

            //Find the placementTarget
            var item = (DataGrid) contextMenu.PlacementTarget;

            //Get the underlying item, that you cast to your object that is bound
            //to the DataGrid (and has subject and state as property)
            var buddySelect = (PokemonDataViewModel) item.SelectedCells[0].Item;

            Task.Run(async () =>
            {
                await SelectBuddyPokemonTask.Execute(
                    this.Session,
                    this.Session.CancellationTokenSource.Token,
                    buddySelect.Id);
            });
        }

        //ICommand transferPokemonCommand;
        //public ICommand TransferPokemonCommand
        //{
        //    get
        //    {
        //        if (transferPokemonCommand == null)
        //        {
        //            transferPokemonCommand = new RelayCommand(param => this.ShowCustomer());
        //        }
        //        return transferPokemonCommand;
        //    }
        //}
    }
}