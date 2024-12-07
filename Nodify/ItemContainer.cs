﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Nodify
{
    /// <summary>
    /// Delegate used to notify when an <see cref="ItemContainer"/> is previewing a new location.
    /// </summary>
    /// <param name="newLocation">The new location.</param>
    public delegate void PreviewLocationChanged(Point newLocation);

    /// <summary>
    /// The container for all the items generated by the <see cref="ItemsControl.ItemsSource"/> of the <see cref="NodifyEditor"/>.
    /// </summary>
    public class ItemContainer : ContentControl, INodifyCanvasItem
    {
        #region Dependency Properties

        public static readonly DependencyProperty HighlightBrushProperty = DependencyProperty.Register(nameof(HighlightBrush), typeof(Brush), typeof(ItemContainer));
        public static readonly DependencyProperty SelectedBrushProperty = DependencyProperty.Register(nameof(SelectedBrush), typeof(Brush), typeof(ItemContainer));
        public static readonly DependencyProperty SelectedBorderThicknessProperty = DependencyProperty.Register(nameof(SelectedBorderThickness), typeof(Thickness), typeof(ItemContainer), new FrameworkPropertyMetadata(BoxValue.Thickness2));
        public static readonly DependencyProperty IsSelectableProperty = DependencyProperty.Register(nameof(IsSelectable), typeof(bool), typeof(ItemContainer), new FrameworkPropertyMetadata(BoxValue.True));
        public static readonly DependencyProperty IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(typeof(ItemContainer), new FrameworkPropertyMetadata(BoxValue.False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSelectedChanged));
        protected static readonly DependencyPropertyKey IsPreviewingSelectionPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsPreviewingSelection), typeof(bool?), typeof(ItemContainer), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty IsPreviewingSelectionProperty = IsPreviewingSelectionPropertyKey.DependencyProperty;
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(nameof(Location), typeof(Point), typeof(ItemContainer), new FrameworkPropertyMetadata(BoxValue.Point, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnLocationChanged));
        public static readonly DependencyProperty ActualSizeProperty = DependencyProperty.Register(nameof(ActualSize), typeof(Size), typeof(ItemContainer), new FrameworkPropertyMetadata(BoxValue.Size));
        public static readonly DependencyProperty DesiredSizeForSelectionProperty = DependencyProperty.Register(nameof(DesiredSizeForSelection), typeof(Size?), typeof(ItemContainer), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.NotDataBindable));
        private static readonly DependencyPropertyKey IsPreviewingLocationPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsPreviewingLocation), typeof(bool), typeof(ItemContainer), new FrameworkPropertyMetadata(BoxValue.False));
        public static readonly DependencyProperty IsPreviewingLocationProperty = IsPreviewingLocationPropertyKey.DependencyProperty;
        public static readonly DependencyProperty IsDraggableProperty = DependencyProperty.Register(nameof(IsDraggable), typeof(bool), typeof(ItemContainer), new FrameworkPropertyMetadata(BoxValue.True));
        public static readonly DependencyProperty HasCustomContextMenuProperty = NodifyEditor.HasCustomContextMenuProperty.AddOwner(typeof(ItemContainer));

        private static void OnLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (ItemContainer)d;
            item.OnLocationChanged();

            if (item.Editor.IsLoaded && !item.Editor.IsBulkUpdatingItems)
            {
                item.Editor.ItemsHost.InvalidateArrange();
            }
        }

        /// <summary>
        /// Gets or sets the brush used when the <see cref="PendingConnection.IsOverElementProperty"/> attached property is true for this <see cref="ItemContainer"/>.
        /// </summary>
        public Brush HighlightBrush
        {
            get => (Brush)GetValue(HighlightBrushProperty);
            set => SetValue(HighlightBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the brush used when <see cref="IsSelected"/> or <see cref="IsPreviewingSelection"/> is true.
        /// </summary>
        public Brush SelectedBrush
        {
            get => (Brush)GetValue(SelectedBrushProperty);
            set => SetValue(SelectedBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the border thickness used when <see cref="IsSelected"/> or <see cref="IsPreviewingSelection"/> is true.
        /// </summary>
        public Thickness SelectedBorderThickness
        {
            get => (Thickness)GetValue(SelectedBorderThicknessProperty);
            set => SetValue(SelectedBorderThicknessProperty, value);
        }

        /// <summary>
        /// Gets or sets the location of this <see cref="ItemContainer"/> inside the <see cref="NodifyEditor"/> in graph space coordinates.
        /// </summary>
        public Point Location
        {
            get => (Point)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        /// <summary>
        /// Gets or sets a value that indicates whether this <see cref="ItemContainer"/> is selected.
        /// Can only be set if <see cref="IsSelectable"/> is true.
        /// </summary>
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ItemContainer"/> is about to change its <see cref="IsSelected"/> state.
        /// </summary>
        public bool? IsPreviewingSelection
        {
            get => (bool?)GetValue(IsPreviewingSelectionProperty);
            internal set => SetValue(IsPreviewingSelectionPropertyKey, value);
        }

        /// <summary>
        /// Gets or sets whether this <see cref="ItemContainer"/> can be selected.
        /// </summary>
        public bool IsSelectable
        {
            get => (bool)GetValue(IsSelectableProperty);
            set => SetValue(IsSelectableProperty, value);
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ItemContainer"/> is previewing a new location but didn't logically move there.
        /// </summary>
        public bool IsPreviewingLocation
        {
            get => (bool)GetValue(IsPreviewingLocationProperty);
            protected internal set => SetValue(IsPreviewingLocationPropertyKey, value);
        }

        /// <summary>
        /// Gets the actual size of this <see cref="ItemContainer"/>.
        /// </summary>
        public Size ActualSize
        {
            get => (Size)GetValue(ActualSizeProperty);
            set => SetValue(ActualSizeProperty, value);
        }

        /// <summary>
        /// Overrides the size to check against when calculating if this <see cref="ItemContainer"/> can be part of the current <see cref="NodifyEditor.SelectedArea"/>.
        /// Defaults to <see cref="UIElement.RenderSize"/>.
        /// </summary>
        public Size? DesiredSizeForSelection
        {
            get => (Size?)GetValue(DesiredSizeForSelectionProperty);
            set => SetValue(DesiredSizeForSelectionProperty, value);
        }

        /// <summary>
        /// Gets or sets whether this <see cref="ItemContainer"/> can be dragged.
        /// </summary>
        public bool IsDraggable
        {
            get => (bool)GetValue(IsDraggableProperty);
            set => SetValue(IsDraggableProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the container uses a custom context menu.
        /// </summary>
        /// <remarks>When set to true, the container handles the right-click event for specific operations.</remarks>
        public bool HasCustomContextMenu
        {
            get => (bool)GetValue(HasCustomContextMenuProperty);
            set => SetValue(HasCustomContextMenuProperty, value);
        }

        /// <summary>
        /// Gets a value indicating whether the container has a context menu.
        /// </summary>
        public bool HasContextMenu => ContextMenu != null || HasCustomContextMenu;

        #endregion

        #region Routed Events

        public static readonly RoutedEvent SelectedEvent = Selector.SelectedEvent.AddOwner(typeof(ItemContainer));
        public static readonly RoutedEvent UnselectedEvent = Selector.UnselectedEvent.AddOwner(typeof(ItemContainer));
        public static readonly RoutedEvent LocationChangedEvent = EventManager.RegisterRoutedEvent(nameof(LocationChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ItemContainer));

        /// <summary>
        /// Occurs when the <see cref="Location"/> of this <see cref="ItemContainer"/> is changed.
        /// </summary>
        public event RoutedEventHandler LocationChanged
        {
            add => AddHandler(LocationChangedEvent, value);
            remove => RemoveHandler(LocationChangedEvent, value);
        }

        /// <summary>
        /// Occurs when this <see cref="ItemContainer"/> is selected.
        /// </summary>
        public event RoutedEventHandler Selected
        {
            add => AddHandler(SelectedEvent, value);
            remove => RemoveHandler(SelectedEvent, value);
        }

        /// <summary>
        /// Occurs when this <see cref="ItemContainer"/> is unselected.
        /// </summary>
        public event RoutedEventHandler Unselected
        {
            add => AddHandler(UnselectedEvent, value);
            remove => RemoveHandler(UnselectedEvent, value);
        }

        /// <summary>
        /// Raises the <see cref="LocationChangedEvent"/> and sets <see cref="IsPreviewingLocation"/> to false.
        /// </summary>
        protected void OnLocationChanged()
        {
            IsPreviewingLocation = false;
            RaiseEvent(new RoutedEventArgs(LocationChangedEvent, this));
        }

        /// <summary>
        /// Raises the <see cref="SelectedEvent"/> or <see cref="UnselectedEvent"/> based on <paramref name="newValue"/>.
        /// Called when the <see cref="IsSelected"/> value is changed.
        /// </summary>
        /// <param name="newValue">True if selected, false otherwise.</param>
        protected void OnSelectedChanged(bool newValue)
        {
            // Don't raise the event if the editor is selecting
            if (!Editor.IsSelecting)
            {
                RaiseEvent(new RoutedEventArgs(newValue ? SelectedEvent : UnselectedEvent, this));
            }
        }

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var elem = (ItemContainer)d;
            bool result = elem.IsSelectable && (bool)e.NewValue;
            elem.IsSelected = result;
            elem.OnSelectedChanged(result);
        }

        #endregion

        #region Fields

        /// <summary>
        /// Indicates whether right-click on the container should preserve the current selection. 
        /// </summary>
        /// <remarks>Has no effect if the container has a context menu.</remarks>
        public static bool PreserveSelectionOnRightClick { get; set; }

        /// <summary>
        /// The <see cref="NodifyEditor"/> that owns this <see cref="ItemContainer"/>.
        /// </summary>
        public NodifyEditor Editor { get; }

        /// <summary>
        /// The calculated margin when the container is selected or previewing selection.
        /// </summary>
        public Thickness SelectedMargin => new Thickness(
            BorderThickness.Left - SelectedBorderThickness.Left,
            BorderThickness.Top - SelectedBorderThickness.Top,
            BorderThickness.Right - SelectedBorderThickness.Right,
            BorderThickness.Bottom - SelectedBorderThickness.Bottom);

        #endregion

        /// <summary>
        /// Occurs when the <see cref="ItemContainer"/> is previewing a new location.
        /// </summary>
        public event PreviewLocationChanged? PreviewLocationChanged;

        /// <summary>
        /// Raises the <see cref="PreviewLocationChanged"/> event and sets the <see cref="IsPreviewingLocation"/> property to true.
        /// </summary>
        /// <param name="newLocation">The new location.</param>
        protected internal void OnPreviewLocationChanged(Point newLocation)
        {
            IsPreviewingLocation = true;
            PreviewLocationChanged?.Invoke(newLocation);
        }

        static ItemContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemContainer), new FrameworkPropertyMetadata(typeof(ItemContainer)));
            FocusableProperty.OverrideMetadata(typeof(ItemContainer), new FrameworkPropertyMetadata(BoxValue.True));
        }

        /// <summary>
        /// Constructs an instance of an <see cref="ItemContainer"/> in the specified <see cref="NodifyEditor"/>.
        /// </summary>
        /// <param name="editor"></param>
        public ItemContainer(NodifyEditor editor)
        {
            Editor = editor;

            InputProcessor.AddHandler(new ContainerDefaultState(this));
        }

        /// <inheritdoc />
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            ActualSize = sizeInfo.NewSize;
            base.OnRenderSizeChanged(sizeInfo);
        }

        /// <summary>
        /// Checks if <paramref name="position"/> is selectable.
        /// </summary>
        /// <param name="position">A position relative to this <see cref="ItemContainer"/>.</param>
        /// <returns>True if <paramref name="position"/> is selectable.</returns>
        protected internal virtual bool IsSelectableLocation(Point position)
        {
            Size size = DesiredSizeForSelection ?? RenderSize;
            return position.X >= 0 && position.Y >= 0 && position.X <= size.Width && position.Y <= size.Height;
        }

        /// <summary>
        /// Checks if <paramref name="area"/> contains or intersects with this <see cref="ItemContainer"/> taking into consideration the <see cref="DesiredSizeForSelection"/>.
        /// </summary>
        /// <param name="area">The area to check if contains or intersects this <see cref="ItemContainer"/>.</param>
        /// <param name="isContained">If true will check if <paramref name="area"/> contains this, otherwise will check if <paramref name="area"/> intersects with this.</param>
        /// <returns>True if <paramref name="area"/> contains or intersects this <see cref="ItemContainer"/>.</returns>
        public virtual bool IsSelectableInArea(Rect area, bool isContained)
        {
            var bounds = new Rect(Location, DesiredSizeForSelection ?? RenderSize);
            return isContained ? area.Contains(bounds) : area.IntersectsWith(bounds);
        }

        /// <inheritdoc cref="NodifyEditor.BeginDragging()" />
        /// <remarks>Replaces the selection if the container is not selected.</remarks>
        public void BeginDragging()
        {
            if (!IsSelected)
            {
                Select(SelectionType.Replace);
            }

            Editor.BeginDragging();
        }

        /// <inheritdoc cref="NodifyEditor.UpdateDragging(Vector)" />
        public void UpdateDragging(Vector amount)
            => Editor.UpdateDragging(amount);

        /// <inheritdoc cref="NodifyEditor.CancelDragging" />
        public void CancelDragging()
            => Editor.CancelDragging();

        /// <inheritdoc cref="NodifyEditor.EndDragging" />
        public void EndDragging()
            => Editor.EndDragging();

        /// <summary>
        /// Modifies the selection state of the current item based on the specified selection type.
        /// </summary>
        /// <param name="type">The type of selection to perform.</param>
        public void Select(SelectionType type)
        {
            switch (type)
            {
                case SelectionType.Append:
                    IsSelected = true;
                    break;
                case SelectionType.Remove:
                    IsSelected = false;
                    break;
                case SelectionType.Invert:
                    IsSelected = !IsSelected;
                    break;
                case SelectionType.Replace:
                    Editor.UnselectAll();
                    IsSelected = true;
                    break;
            }
        }

        #region Gesture Handling

        protected InputProcessor InputProcessor { get; } = new InputProcessor { ProcessHandledEvents = true };

        /// <inheritdoc />
        protected override void OnMouseDown(MouseButtonEventArgs e)
            => InputProcessor.Process(e);

        /// <inheritdoc />
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            InputProcessor.Process(e);

            // Release the mouse capture if all the mouse buttons are released
            if (IsMouseCaptured && e.RightButton == MouseButtonState.Released && e.LeftButton == MouseButtonState.Released && e.MiddleButton == MouseButtonState.Released)
            {
                ReleaseMouseCapture();
            }
        }

        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs e)
            => InputProcessor.Process(e);

        /// <inheritdoc />
        protected override void OnMouseWheel(MouseWheelEventArgs e)
            => InputProcessor.Process(e);

        /// <inheritdoc />
        protected override void OnLostMouseCapture(MouseEventArgs e)
            => InputProcessor.Process(e);

        /// <inheritdoc />
        protected override void OnKeyUp(KeyEventArgs e)
            => InputProcessor.Process(e);

        /// <inheritdoc />
        protected override void OnKeyDown(KeyEventArgs e)
            => InputProcessor.Process(e);

        #endregion
    }
}
