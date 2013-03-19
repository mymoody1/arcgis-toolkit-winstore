﻿// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using ESRI.ArcGIS.Runtime.Symbology;
using ESRI.ArcGIS.Runtime.Toolkit.Xaml.Primitives;
using ESRI.ArcGIS.Runtime.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ESRI.ArcGIS.Runtime.Toolkit.Xaml
{
	/// <summary>
	/// The Legend is a utility control that displays the symbology of Layers and their description for map 
	/// interpretation.
	/// </summary>
	public class Legend : Control
	{
		#region Constructor
		private bool _isLoaded;
		private readonly LegendTree _legendTree;

		/// <summary>
		/// Initializes a new instance of the <see cref="Legend"/> class.
		/// </summary>
		public Legend()
		{
			DefaultStyleKey = typeof(Legend);
			_legendTree = new LegendTree();
			_legendTree.Refreshed += OnRefreshed;
			_legendTree.PropertyChanged += LegendTree_PropertyChanged;
		}

		void LegendTree_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "LayerItemsSource")
				LayerItemsSource = _legendTree.LayerItemsSource;
			else if (e.PropertyName == "LayerItems")
				LayerItems = _legendTree.LayerItems;
		}

		#endregion

		#region Map

		/// <summary>
		/// Gets or sets the map whose layers it should display a legend for.
		/// </summary>
		public Map Map
		{
			get { return GetValue(MapProperty) as Map; }
			set { SetValue(MapProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="Map"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty MapProperty =
			DependencyProperty.Register("Map", typeof(Map), typeof(Legend), new PropertyMetadata(null, OnMapPropertyChanged));

		private static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((Legend) d).OnMapPropertyChanged(e.NewValue as Map);
		}

		private void OnMapPropertyChanged(Map newMap)
		{
			if (!_isLoaded)
				return; // defer initialization until all parameters are well known

			_legendTree.Map = newMap;
		}

		#endregion

		#region LayerItemsMode

		#region Mode
		/// <summary>
		/// LayerItems mode enumeration defines the structure of the legend : Flat or Tree.
		/// </summary>
		public enum Mode
		{
			/// <summary>
			/// Flat structure : LayerItemsSource returns the LayerItems leaves (i.e not the group layers nor the map layers with sub layers) and the LegendItems. 
			/// <remarks>This is the default value.</remarks>
			/// </summary>
			Flat,
			/// <summary>
			/// Tree structure : LayerItemsSource returns a hierarchy of legend items taking care of the map layers and of the group layers. 
			/// </summary>
			Tree
		};
		#endregion

		/// <summary>
		/// Gets or sets the layer items mode
		/// </summary>
		public Mode LayerItemsMode
		{
			get { return (Mode)GetValue(LayerItemsModeProperty); }
			set { SetValue(LayerItemsModeProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="LayerItemsModeProperty"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty LayerItemsModeProperty =
			DependencyProperty.Register("LayerItemsMode", typeof(Mode), typeof(Legend), new PropertyMetadata(Mode.Flat, OnLayerItemsModePropertyChanged));


		private static void OnLayerItemsModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((Legend) d).OnLayerItemsModePropertyChanged((Mode)e.NewValue);
		}

		private void OnLayerItemsModePropertyChanged(Mode newLayerItemsMode)
		{
			_legendTree.LayerItemsMode = newLayerItemsMode;
		}
		#endregion

		#region ShowOnlyVisibleLayers

		/// <summary>
		/// Gets or sets a value indicating whether only the visible layers are participating to the legend.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if only the visible layers are participating to the legend; otherwise, <c>false</c>.
		/// </value>
		public bool ShowOnlyVisibleLayers
		{
			get { return (bool)GetValue(ShowOnlyVisibleLayersProperty); }
			set { SetValue(ShowOnlyVisibleLayersProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="ShowOnlyVisibleLayers"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ShowOnlyVisibleLayersProperty =
				DependencyProperty.Register("ShowOnlyVisibleLayers", typeof(bool), typeof(Legend), new PropertyMetadata(true, OnShowOnlyVisibleLayersPropertyChanged));

		private static void OnShowOnlyVisibleLayersPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((Legend) d).OnShowOnlyVisibleLayersPropertyChanged((bool)e.NewValue);
		}

		private void OnShowOnlyVisibleLayersPropertyChanged(bool newValue)
		{
			_legendTree.ShowOnlyVisibleLayers = newValue;
		}

		#endregion

		#region LayerItems
		/// <summary>
		/// Gets the LayerItems for all layers that the legend control is working with.
		/// </summary>
		/// <value>The LayerItems.</value>
		public ObservableCollection<LayerItemViewModel> LayerItems
		{
			get { return (ObservableCollection<LayerItemViewModel>)GetValue(LayerItemsProperty); }
			internal set { SetValue(LayerItemsProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="LayerItems"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty LayerItemsProperty =
			DependencyProperty.Register("LayerItems", typeof(ObservableCollection<LayerItemViewModel>), typeof(Legend), null);
		#endregion

		#region LayerItemsSource
		/// <summary>
		/// The enumeration of the legend items displayed at the first level of the legend control.
		/// This enumeration is depending on the <see cref="Legend.LayerItemsMode"/> property and on the <see cref="Legend.ShowOnlyVisibleLayers"/> property.
		/// </summary>
		/// <value>The layer items source.</value>
		public IEnumerable<LegendItemViewModel> LayerItemsSource
		{
			get { return (IEnumerable<LegendItemViewModel>)GetValue(LayerItemsSourceProperty); }
			internal set { SetValue(LayerItemsSourceProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="LayerItemsSource"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty LayerItemsSourceProperty =
			DependencyProperty.Register("LayerItemsSource", typeof(IEnumerable<LegendItemViewModel>), typeof(Legend), null);
		#endregion

		#region LegendItemTemplate

		/// <summary>
		/// Gets or sets the legend item template
		/// </summary>
		public DataTemplate LegendItemTemplate
		{
			get { return (DataTemplate)GetValue(LegendItemTemplateProperty); }
			set { SetValue(LegendItemTemplateProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="LegendItemTemplate"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty LegendItemTemplateProperty =
			DependencyProperty.Register("LegendItemTemplate", typeof(DataTemplate), typeof(Legend), new PropertyMetadata(null, OnLegendItemTemplateChanged));

		private static void OnLegendItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((Legend) d).OnLegendItemTemplateChanged(e.NewValue as DataTemplate);
		}

		private void OnLegendItemTemplateChanged(DataTemplate newDataTemplate)
		{
			_legendTree.LegendItemTemplate = newDataTemplate;
		}

		#endregion

		#region LayerTemplate
		/// <summary>
		/// Gets or sets the layer template
		/// </summary>
		public DataTemplate LayerTemplate
		{
			get { return (DataTemplate)GetValue(LayerTemplateProperty); }
			set { SetValue(LayerTemplateProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="LayerTemplate"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty LayerTemplateProperty =
			DependencyProperty.Register("LayerTemplate", typeof(DataTemplate), typeof(Legend), new PropertyMetadata(null, OnLayerTemplateChanged));

		private static void OnLayerTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((Legend) d).OnLayerTemplateChanged(e.NewValue as DataTemplate);
		}

		private void OnLayerTemplateChanged(DataTemplate newDataTemplate)
		{
			_legendTree.LayerTemplate = newDataTemplate;

		}
		#endregion

		#region ReverseLayersOrder

		/// <summary>
		/// Gets or sets a property specifying whether the order the layers are 
		/// processed in should be reversed.
		/// </summary>
		public bool ReverseLayersOrder
		{
			get { return (bool)GetValue(ReverseLayersOrderProperty); }
			set { SetValue(ReverseLayersOrderProperty, value); }
		}

		/// /// <summary>
		/// Identifies the <see cref="ReverseLayersOrder"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ReverseLayersOrderProperty =
			DependencyProperty.Register("ReverseLayersOrder", typeof(bool), typeof(Legend), new PropertyMetadata(false, OnReverseLayersOrderChanged));

		private static void OnReverseLayersOrderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((Legend) d).OnReverseLayersOrderChanged((bool)e.NewValue);
		}

		private void OnReverseLayersOrderChanged(bool newReverseLayersOrder)
		{
			_legendTree.ReverseLayersOrder = newReverseLayersOrder;

		}
		#endregion

		#region MapLayerTemplate

		/// <summary>
		/// Gets or sets the map layer template
		/// </summary>
		public DataTemplate MapLayerTemplate
		{
			get { return (DataTemplate)GetValue(MapLayerTemplateProperty); }
			set { SetValue(MapLayerTemplateProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="LegendItemTemplate"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty MapLayerTemplateProperty =
			DependencyProperty.Register("MapLayerTemplate", typeof(DataTemplate), typeof(Legend), new PropertyMetadata(null, OnMapLayerTemplateChanged));

		private static void OnMapLayerTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((Legend) d).OnMapLayerTemplateChanged(e.NewValue as DataTemplate);
		}

		private void OnMapLayerTemplateChanged(DataTemplate newDataTemplate)
		{
			_legendTree.MapLayerTemplate = newDataTemplate;
		}
		#endregion

		#region Refresh
		/// <summary>
		/// Refreshes the legend control.
		/// </summary>
		/// <remarks>Note : In most cases, the control is always up to date without calling the refresh method.</remarks>
		public void Refresh()
		{
			_legendTree.Refresh();
		}
		#endregion

		#region public override void OnApplyTemplate()
		/// <summary>
		/// Invoked whenever application code or internal processes (such as a 
		/// rebuilding layout pass) call ApplyTemplate. In simplest terms, this
		/// means the method is called just before a UI element displays in your
		/// app. Override this method to influence the default post-template 
		/// logic of a class.
		/// </summary>
		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (!_isLoaded)
			{
				_isLoaded = true;

				if (Windows.ApplicationModel.DesignMode.DesignModeEnabled && (Map == null || Map.Layers == null || !Map.Layers.Any()))
				{
					if (_legendTree.LayerItems == null)
					{
						// Create a basic hierachy for design :  Map Layer -> SubLayer -> LegendItemViewModel
						var legendItem1 = new LegendItemViewModel
						{
							Label = "LegendItem1",
							Symbol = new SimpleMarkerSymbol { Style = SimpleMarkerStyle.Circle, Color = Colors.Red}
						};
						var legendItem2 = new LegendItemViewModel
						{
							Label = "LegendItem2",
							Symbol = new SimpleMarkerSymbol { Style = SimpleMarkerStyle.Diamond, Color = Colors.Green }
						};

						var layerItem = new LayerItemViewModel
						{
							Label = "LayerItem",
							LegendItems = new ObservableCollection<LegendItemViewModel> { legendItem1, legendItem2 }
						};

						var mapLayerItem = new LayerItemViewModel
						{
							Label = "MapLayerItem",
							LayerType = MapLayerItem.MapLayerType,
							LayerItems = new ObservableCollection<LayerItemViewModel> { layerItem },
						};

						_legendTree.LayerItems = new ObservableCollection<LayerItemViewModel> { mapLayerItem };
					}

				}
				else
				{
					// Initialize the Map now that all parameters are well known
					_legendTree.Map = Map;
				}
			}
		}

		#endregion

		#region Event Refreshed
		/// <summary>
		/// Occurs when the legend is refreshed. 
		/// Give the opportunity for an application to add or remove legend items.
		/// </summary>
		public event EventHandler<RefreshedEventArgs> Refreshed;

		private void OnRefreshed(object sender, RefreshedEventArgs args)
		{
			EventHandler<RefreshedEventArgs> refreshed = Refreshed;

			if (refreshed != null)
			{
				refreshed(this, args);
			}
		}
		#endregion

		#region class RefreshedEventArgs
		/// <summary>
		/// Legend Event Arguments used when the legend is refreshed.
		/// </summary>
		public sealed class RefreshedEventArgs : EventArgs
		{
			internal RefreshedEventArgs(LayerItemViewModel layerItem, Exception ex)
			{
				LayerItem = layerItem;
				Error = ex;
			}

			/// <summary>
			/// Gets the layer item being refreshed.
			/// </summary>
			/// <value>The layer item.</value>
			public LayerItemViewModel LayerItem { get; internal set; }

			/// <summary>
			/// Gets a value that indicates which error occurred during the legend refresh.
			/// </summary>
			/// <value>An System.Exception instance, if an error occurred during the refresh; otherwise null.</value>
			public Exception Error { get; internal set; }
		} 
		#endregion
	}
}
