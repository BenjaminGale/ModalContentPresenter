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
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace BenjaminGale.Controls
{
    /// <summary>
    /// Allows the display of modal content over another piece of content.
    /// </summary>
    [ContentProperty("Content")]
    public class ModalContentPresenter : FrameworkElement
    {
        #region private fields

        private Panel layoutRoot;                               // Used to layout the content and modal content.
        private ContentPresenter primaryContentPresenter;       // Hosts the primary content.
        private ContentPresenter modalContentPresenter;         // Hosts the modal content.

        /*
         * This covers the primary content whilst the modal content is being shown which
         * stops the user from being able to select the primary content using the mouse.
         * By default the overlay is DarkGray with 80% opacity but this can be changed
         * using the 'OverlayBrush' property.
         */
        private Border overlay;

        /*
         * This is required for tracking the logical elements that make up this custom 
         * element. These will be used to help WPF construct the logical tree.
         * This will keep track of the content contained in the primaryContent
         * and modal content ContentPresenters (The ContentPresenters should not be part of
         * the logical tree because they are an implementation detail of this element).
         */
        private object[] logicalChildren;

        /*
         * When the modal content is displayed, the keyboard navigation mode of the primary 
         * content is cached and then set to 'none'. When the modal content is hidden, the 
         * keyboard navigation mode of the primary content is set back to the cached value.
         * This stops the user being able to 'tab' into the primary content whilst the modal
         * content is being displayed and restore the default value once the modal content
         * is hidden.
         */
        private KeyboardNavigationMode cachedKeyboardNavigationMode;

        /*
         * When modal content is shown or hidden, focus needs to be set on the 'next' element.
         * Because the 'Content' and 'ModalContent' properties take an object it is not possible to
         * know what is being displayed. When the ModalContentPresenter is instantiated, this field 
         * will be set to the value of 'FocusNavigationDirection.First' which means that focus will 
         * be set to the first logical element in whatever content is being displayed.
         */
        private static readonly TraversalRequest traversalDirection;

        #endregion

        #region dependency properties

        /// <summary>
        /// Identifies the ModalContentPresenter.IsModal dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the ModalContentPresenter.IsModal dependency property.
        /// </returns>
        public static readonly DependencyProperty IsModalProperty =
            DependencyProperty.Register("IsModal", typeof(bool), typeof(ModalContentPresenter),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnIsModalChanged));

        /// <summary>
        /// Identifies the ModalContentPresenter.Content dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the ModalContentPresenter.Content dependency property.
        /// </returns>
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(ModalContentPresenter),
            new UIPropertyMetadata(null, OnContentChanged));

        /// <summary>
        /// Identifies the ModalContentPresenter.ModalContent dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the ModalContentPresenter.ModalContent dependency property.
        /// </returns>
        public static readonly DependencyProperty ModalContentProperty =
            DependencyProperty.Register("ModalContent", typeof(object), typeof(ModalContentPresenter),
            new UIPropertyMetadata(null, OnModalContentChanged));

        /// <summary>
        /// Identifies the ModalContentPresenter.OverlayBrush dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the ModalContentPresenter.OverlayBrush dependency property.
        /// </returns>
        public static readonly DependencyProperty OverlayBrushProperty = 
            DependencyProperty.Register("OverlayBrush", typeof(Brush), typeof(ModalContentPresenter),
            new UIPropertyMetadata(new SolidColorBrush(Color.FromArgb(204,169,169,169)),
                OnOverlayBrushChanged));

        /// <summary>
        /// Gets or sets a value that indicates whether modal content is currently being displayed.
        /// </summary>
        public bool IsModal
        {
            get { return (bool)GetValue(IsModalProperty); }
            set { SetValue(IsModalProperty, value); }
        }

        /// <summary>
        /// Gets or sets the primary content of the ModalContentPresenter. 
        /// </summary>
        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// Gets or sets the modal content of the ModalContentPresenter.
        /// </summary>
        public object ModalContent
        {
            get { return (object)GetValue(ModalContentProperty); }
            set { SetValue(ModalContentProperty, value); }
        }

        /// <summary>
        /// Gets or sets a brush that describes the overlay that is displayed when the modal content is being shown.
        /// </summary>
        public Brush OverlayBrush
        {
            get { return (Brush)GetValue(OverlayBrushProperty); }
            set { SetValue(OverlayBrushProperty, value); }
        }

        #endregion

        #region routed events

        /// <summary>
        /// Identifies the ModalContentPresenter.PreviewModalContentShown routed event.
        /// </summary>
        /// <returns>
        /// The identifier for the ModalContentPresenter.PreviewModalContentShown routed event.
        /// </returns>
        public static readonly RoutedEvent PreviewModalContentShownEvent =
            EventManager.RegisterRoutedEvent("PreviewModalContentShown",
            RoutingStrategy.Tunnel,
            typeof(RoutedEventArgs),
            typeof(ModalContentPresenter));

        /// <summary>
        /// Identifies the ModalContentPresenter.ModalContentShown routed event.
        /// </summary>
        /// <returns>
        /// The identifier for the ModalContentPresenter.ModalContentShown routed event.
        /// </returns>
        public static readonly RoutedEvent ModalContentShownEvent =
            EventManager.RegisterRoutedEvent("ModalContentShown",
            RoutingStrategy.Bubble, 
            typeof(RoutedEventArgs), 
            typeof(ModalContentPresenter));

        /// <summary>
        /// Identifies the ModalContentPresenter.PreviewModalContentHidden routed event.
        /// </summary>
        /// <returns>
        /// The identifier for the ModalContentPresenter.PreviewModalContentHidden routed event.
        /// </returns>
        public static readonly RoutedEvent PreviewModalContentHiddenEvent =
            EventManager.RegisterRoutedEvent("PreviewModalContentHidden",
            RoutingStrategy.Tunnel,
            typeof(RoutedEventArgs),
            typeof(ModalContentPresenter));

        /// <summary>
        /// Identifies the ModalContentPresenter.ModalContentHidden routed event.
        /// </summary>
        /// <returns>
        /// The identifier for the ModalContentPresenter.ModalContentHidden routed event.
        /// </returns>
        public static readonly RoutedEvent ModalContentHiddenEvent =
            EventManager.RegisterRoutedEvent("ModalContentHidden",
            RoutingStrategy.Bubble,
            typeof(RoutedEventArgs),
            typeof(ModalContentPresenter));

        /// <summary>
        /// Occurs when the modal content is shown.
        /// </summary>
        public event RoutedEventHandler PreviewModalContentShown
        {
            add { AddHandler(PreviewModalContentShownEvent, value); }
            remove { RemoveHandler(PreviewModalContentShownEvent, value); }
        }

        /// <summary>
        /// Occurs when the modal content is shown.
        /// </summary>
        public event RoutedEventHandler ModalContentShown
        {
            add { AddHandler(ModalContentShownEvent, value); }
            remove { RemoveHandler(ModalContentShownEvent, value); }
        }

        /// <summary>
        /// Occurs when the modal content is hidden.
        /// </summary>
        public event RoutedEventHandler PreviewModalContentHidden
        {
            add { AddHandler(PreviewModalContentHiddenEvent, value); }
            remove { RemoveHandler(PreviewModalContentHiddenEvent, value); }
        }

        /// <summary>
        /// Occurs when the modal content is hidden.
        /// </summary>
        public event RoutedEventHandler ModalContentHidden
        {
            add { AddHandler(ModalContentHiddenEvent, value); }
            remove { RemoveHandler(ModalContentHiddenEvent, value); }
        }

        #endregion

        #region command bindings

        /// <summary>
        /// Add CommandBindings
        /// </summary>
        private void CreateCommands()
        {
            CommandBinding showModalCommandBinding = new CommandBinding(ModalContentCommands.ShowModalContent, (sender, args) => this.ShowModalContent(), (sender, args) => args.CanExecute = !this.IsModal);
            this.CommandBindings.Add(showModalCommandBinding);

            CommandBinding hideModalCommandBinding = new CommandBinding(ModalContentCommands.HideModalContent, (sender, args) => this.HideModalContent(), (sender, args) => args.CanExecute = this.IsModal);
            this.CommandBindings.Add(hideModalCommandBinding);
        }

        #endregion

        #region ModalContentPresenter implementation

        static ModalContentPresenter()
        {
            traversalDirection = new TraversalRequest(FocusNavigationDirection.First);
        }

        /// <summary>
        /// Initializes a new instance of the ModalContentPresenter class.
        /// </summary>
        public ModalContentPresenter()
        {
            layoutRoot = new ModalContentPresenterPanel();
            primaryContentPresenter = new ContentPresenter();
            modalContentPresenter = new ContentPresenter();
            overlay = new Border();

            AddVisualChild(layoutRoot);

            logicalChildren = new object[2];

            overlay.Background = OverlayBrush;
            overlay.Child = modalContentPresenter;
            overlay.Visibility = Visibility.Hidden;

            layoutRoot.Children.Add(primaryContentPresenter);
            layoutRoot.Children.Add(overlay);

            CreateCommands();
        }

        /// <summary>
        /// Shows the modal content over the primary content.
        /// If the modal content is already being shown, this method does nothing.
        /// </summary>
        public void ShowModalContent()
        {
            if(!IsModal) 
                IsModal= true;
        }

        /// <summary>
        /// Hides the modal content that is displayed over the primary content.
        /// If the modal content is already hidden, this method does nothing.
        /// </summary>
        public void HideModalContent()
        {
            if (IsModal)
                IsModal = false;
        }

        /// <summary>
        /// Raises the PreviewContentShown and ContentShown events.
        /// </summary>
        private void RaiseModalContentShownEvents()
        {
            RoutedEventArgs args = new RoutedEventArgs(PreviewModalContentShownEvent);
            OnPreviewModalContentShown(args);
            if (!args.Handled)
            {
                args = new RoutedEventArgs(ModalContentShownEvent);
                OnModalContentShown(args);
            }
        }

        /// <summary>
        /// Raises the PreviewContentHidden and ContentHidden events.
        /// </summary>
        private void RaiseModalContentHiddenEvents()
        {
            RoutedEventArgs args = new RoutedEventArgs(PreviewModalContentHiddenEvent);
            OnPreviewModalContentHidden(args);
            if (!args.Handled)
            {
                args = new RoutedEventArgs(ModalContentHiddenEvent);
                OnModalContentHidden(args);
            }
        }

        /// <summary>
        /// Raises the ModalContentPresenter.PreviewModalContentShown routed event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected virtual void OnPreviewModalContentShown(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        /// <summary>
        /// Raises the ModalContentPresenter.ModalContentShown routed event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected virtual void OnModalContentShown(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        /// <summary>
        /// Raises the ModalContentPresenter.PreviewModalContentHidden routed event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected virtual void OnPreviewModalContentHidden(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        /// <summary>
        /// Raises the ModalContentPresenter.ModalContentHidden routed event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected virtual void OnModalContentHidden(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        #endregion

        #region property changed callbacks

        private static void OnIsModalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModalContentPresenter control = (ModalContentPresenter)d;

            if ((bool)e.NewValue == true)
            {
                /*
                 * Cache the keyboard navigation mode of the primary content before setting it to
                 * 'none' so that it can be restored when the modal content is hidden.
                 */
                control.cachedKeyboardNavigationMode = KeyboardNavigation.GetTabNavigation(control.primaryContentPresenter);
                KeyboardNavigation.SetTabNavigation(control.primaryContentPresenter, KeyboardNavigationMode.None);

                /*
                 * Show the overlay (which in turn shows the modal content as it is a child of
                 * the overlay) and move focus to the first logical element.
                 */
                control.overlay.Visibility = Visibility.Visible;
                control.overlay.MoveFocus(traversalDirection);

                control.RaiseModalContentShownEvents();
            }
            else
            {
                /*
                 * Hide the overlay (which also hides the modal content...).
                 */
                control.overlay.Visibility = Visibility.Hidden;

                /*
                 * Restore the cached keyboard navigation value on the primary content and move
                 * focus to its first logical element.
                 */
                KeyboardNavigation.SetTabNavigation(control.primaryContentPresenter, control.cachedKeyboardNavigationMode);
                control.primaryContentPresenter.MoveFocus(traversalDirection);

                control.RaiseModalContentHiddenEvents();
            }
        }

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModalContentPresenter control = (ModalContentPresenter)d;

            /*
             * If the ModalContentPresenter already contains primary content then
             * the existing content will need to be removed from the logical tree.
             */
            if (e.OldValue != null)
            {
                control.RemoveLogicalChild(e.OldValue);
            }

            /*
             * Add the new content to the logical tree of the ModalContentPresenter
             * and update the logicalChildren array so that the correct element is returned
             * when it is requested by WPF.
             */
            control.primaryContentPresenter.Content = e.NewValue;
            control.AddLogicalChild(e.NewValue);
            control.logicalChildren[0] = e.NewValue;
        }

        private static void OnModalContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModalContentPresenter control = (ModalContentPresenter)d;

            /*
             * If the ModalContentPresenter already contains modal content then
             * the existing content will need to be removed from the logical tree.
             */
            if (e.OldValue != null)
            {
                control.RemoveLogicalChild(e.OldValue);
            }

            /*
             * Add the new content to the logical tree of the ModalContentPresenter
             * and update the logicalChildren array so that the correct element is returned
             * when it is requested by WPF.
             */
            control.modalContentPresenter.Content = e.NewValue;
            control.AddLogicalChild(e.NewValue);
            control.logicalChildren[1] = e.NewValue;
        }

        private static void OnOverlayBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModalContentPresenter control = (ModalContentPresenter)d;
            control.overlay.Background = (Brush)e.NewValue;
        }

        #endregion

        #region FrameworkElement overrides

        /*
         * These methods are required as a bare minimum for making a custom
         * FrameworkElement render correctly. These methods get the objects which
         * make up the visual and logical tree (the AddVisualChild and AddLogicalChild
         * methods only setup the relationship between the parent/child objects).
         * 
         * The Arrange and Measure methods simply delegate to the layoutRoot panel which
         * calculates where any content should be placed.
         */

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index > 1)
                throw new ArgumentOutOfRangeException("index");

            return layoutRoot;
        }

        protected override int VisualChildrenCount 
        {
            get { return 1; }
        }

        protected override IEnumerator LogicalChildren
        {
            get { return logicalChildren.GetEnumerator(); }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            layoutRoot.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            layoutRoot.Measure(availableSize);
            return layoutRoot.DesiredSize;
        }

        #endregion

        #region layout panel

        /// <summary>
        /// Defines a basic, lightweight layout panel for the ModalContentPresenter. 
        /// </summary>
        class ModalContentPresenterPanel : Panel
        {
            protected override Size MeasureOverride(Size availableSize)
            {
                Size resultSize = new Size(0, 0);

                foreach (UIElement child in Children)
                {
                    child.Measure(availableSize);
                    resultSize.Width = Math.Max(resultSize.Width, child.DesiredSize.Width);
                    resultSize.Height = Math.Max(resultSize.Height, child.DesiredSize.Height);
                }

                return resultSize;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                foreach (UIElement child in InternalChildren)
                {
                    child.Arrange(new Rect(finalSize));
                }

                return finalSize;
            }
        }

        #endregion
    }
}
