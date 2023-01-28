﻿using Chromatics.Core;
using Chromatics.Extensions.RGB.NET;
using Chromatics.Extensions.RGB.NET.Decorators;
using Chromatics.Helpers;
using Chromatics.Interfaces;
using Chromatics.Models;
using RGB.NET.Core;
using Sharlayan.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Chromatics.Extensions.FFXIVWeatherExtensions;

namespace Chromatics.Layers.DynamicLayers
{
    public class ReactiveWeatherHighlightProcessor : LayerProcessor
    {
        private FFXIVWeatherServiceManual weatherService;
        private string _currentWeather = @"";
        private string _currentZone = @"";
        private bool _reactiveWeatherEffects;

        public override void Process(IMappingLayer layer)
        {
            //Do not apply if currently in Preview mode
            if (MappingLayers.IsPreview()) return;
            
            //Reactive Weather Dynamic Layer Layer Implementation
            var _colorPalette = RGBController.GetActivePalette();
            var weather_color = ColorHelper.ColorToRGBColor(_colorPalette.WeatherUnknownHighlight.Color);
            var weather_brush = new SolidColorBrush(weather_color);
            var _layergroups = RGBController.GetLiveLayerGroups();
            var reactiveWeatherEffects = RGBController.GetEffectsSettings().effect_reactiveweather;


            if (FileOperationsHelper.CheckWeatherDataLoaded() && weatherService == null)
            {
                weatherService = new FFXIVWeatherServiceManual();
            }

            //loop through all LED's and assign to device layer  (Order of LEDs is not important for a highlight layer)
            var surface = RGBController.GetLiveSurfaces();
            var devices = surface.GetDevices(layer.deviceType);

            PublicListLedGroup layergroup;
            var ledArray = devices.SelectMany(d => d).Where(led => layer.deviceLeds.Any(v => v.Value.Equals(led.Id))).ToArray();

            if (_layergroups.ContainsKey(layer.layerID))
            {
                layergroup = _layergroups[layer.layerID].FirstOrDefault();
                layergroup.ZIndex = layer.zindex;
            }
            else
            {
                layergroup = new PublicListLedGroup(surface, ledArray)
                {
                    ZIndex = layer.zindex,
                };

                var lg = new PublicListLedGroup[] { layergroup };
                _layergroups.Add(layer.layerID, lg);

                layergroup.Brush = weather_brush;
                layergroup.Detach();
            }

            if (!layer.Enabled)
            {
                layergroup.Detach();
                return;
            }
            else
            {
                //Process data from FFXIV
                
                var _memoryHandler = GameController.GetGameData();

                if (_memoryHandler?.Reader != null && _memoryHandler.Reader.CanGetActors())
                {
                    var getCurrentPlayer = _memoryHandler.Reader.GetCurrentPlayer();
                    if (getCurrentPlayer.Entity == null) return;

                    var currentZone = ZoneLookup.GetZoneInfo(getCurrentPlayer.Entity.MapTerritory).Name.English;

                    if (currentZone != "???" && currentZone != "")
                    {
                        var currentWeather = weatherService.GetCurrentWeather(currentZone).Item1.ToString();

                        if (_currentWeather != currentWeather || _currentZone != currentZone || _reactiveWeatherEffects != reactiveWeatherEffects || layer.requestUpdate)
                        {
                            //layergroup.Brush = weather_brush;
                            SetReactiveWeather(layergroup, currentZone, currentWeather, weather_brush, _colorPalette);

                            _currentWeather = currentWeather;
                            _currentZone = currentZone;
                        }
                    }
                }
                
            }
            

            //Apply lighting
            if (_reactiveWeatherEffects != reactiveWeatherEffects)
            {
                _reactiveWeatherEffects = reactiveWeatherEffects;
            }

            layergroup.Attach(surface);
            _init = true;
            layer.requestUpdate = false;

        }

        private static void SetReactiveWeather(PublicListLedGroup layer, string zone, string weather, SolidColorBrush weather_brush, PaletteColorModel _colorPalette)
        {
            var color = GetWeatherColor(weather, _colorPalette);
            var reactiveWeatherEffects = RGBController.GetEffectsSettings().effect_reactiveweather;

            //Filter for zone specific special weather
            switch(zone)
            {
                case "Mare Lamentorum":
                    if (weather == "Fair Skies" || weather == "Moon Dust")
                    {
                        if (reactiveWeatherEffects)
                            color = ColorHelper.ColorToRGBColor(_colorPalette.WeatherMoonDustBase.Color);
                        else
                            color = ColorHelper.ColorToRGBColor(_colorPalette.WeatherMoonDustHighlight.Color);
                    }
                    break;
            }

            //Filter for special weather
            switch(weather)
            {
                default:
                    //Apply Standard Lookup Weather
                    weather_brush.Color = color;
                    layer.Brush = weather_brush;
                    break;
            }

            
        }

        public static RGB.NET.Core.Color GetWeatherColor(string weatherType, PaletteColorModel colorPalette)
        {
            var paletteType = typeof(PaletteColorModel);
            var fieldName = @"Weather" + weatherType.Replace(" ", "") + @"Highlight";
            var fieldInfo = paletteType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);

            if (fieldInfo == null)
            {
                Debug.WriteLine($"Unknown Weather Color Model: {fieldName}");
                return ColorHelper.ColorToRGBColor(colorPalette.WeatherUnknownHighlight.Color);
            }

            var colorMapping = (ColorMapping)fieldInfo.GetValue(colorPalette);
            return ColorHelper.ColorToRGBColor(colorMapping.Color);
        }
    }
}
