/*
 * Copyright 2012 Benjamin Gale.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace BenjaminGale.Controls
{
    /// <summary>
    /// Defines common commands for use with the ModalContentPresenter.
    /// </summary>
    public static class ModalContentCommands
    {
        private static RoutedUICommand showModalContent;
        private static RoutedUICommand hideModalContent;

        /// <summary>
        /// Gets the value that represents the show modal content command.
        /// </summary>
        public static RoutedUICommand ShowModalContent
        {
            get
            {
                if (showModalContent == null) 
                {
                    showModalContent = new RoutedUICommand("Show Modal Content", "ShowModalContent", typeof(ModalContentCommands));
                }

                return showModalContent;
            }
        }

        /// <summary>
        /// Gets the value that represents the hide modal content command.
        /// </summary>
        public static RoutedUICommand HideModalContent
        {
            get
            {
                if (hideModalContent == null)
                {
                    hideModalContent = new RoutedUICommand("Hide Modal Content", "HideModalContent", typeof(ModalContentCommands));
                }

                return hideModalContent;
            }
        }
    }
}
