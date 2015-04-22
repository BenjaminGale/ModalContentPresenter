/*
 * Copyright 2015 Benjamin Gale.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BenjaminGale.ModalContentPresenter.TestApplication
{
    public class ViewModel : ObservableObject
    {
        private bool isTiling = false;
        private ObservableCollection<string> items = new ObservableCollection<string>();
        private string modalSelection;
        private string selectedItem;

        public ViewModel()
        {
            this.items.Add("Tab 1");
            this.items.Add("Tab 2");
            this.items.Add("Tab 3");
            this.items.Add("Tab 4");
            this.items.Add("Tab 5");

            this.selectedItem = "Tab 1";
        }

        public ObservableCollection<String> Items
        {
            get { return items; }
        }

        public bool IsTiling
        {
            get { return isTiling; }
            set
            {
                isTiling = value;
                OnPropertyChanged("IsTiling");
            }
        }

        public string SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        public string ModalSelection
        {
            get { return modalSelection; }
            set
            {
                modalSelection = value;
                OnPropertyChanged("ModalSelection");
            }
        }

        public ICommand TileCommand
        {
            get 
            { 
                return new DelegateCommand(p => {
                    ModalSelection = SelectedItem;
                    IsTiling = true;
                }); 
            }
        }

        public ICommand CloseCommand
        {
            get 
            { 
                return new DelegateCommand(p => {
                    SelectedItem = ModalSelection;
                    IsTiling = false;
                }); 
            }
        }

        public ICommand AddCommand
        {
            get
            {
                return new DelegateCommand(p => Items.Add("Tab " + (Items.Count + 1)));
            }
        }
    }
}
