﻿using Chromatics.Enums;
using Chromatics.Extensions;
using Chromatics.Helpers;
using MetroFramework.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chromatics.Forms
{
    public partial class Uc_Palette : UserControl
    {
        private bool toggle;
        private bool MappingGridStartup;
        private MetroToolTip tt_mappings;
        private delegate void ResetMappingsDelegate();

        public Uc_Palette()
        {
            InitializeComponent();
        }
        private void OnLoad(object sender, EventArgs e)
        {
            //Create tooltop manager
            tt_mappings = new MetroToolTip
            {
                ToolTipIcon = ToolTipIcon.Info,
                IsBalloon = true,
                ShowAlways = true
            };

            tt_mappings.SetToolTip(btn_paletteexport, @"Export Chromatics color palette to file");
            tt_mappings.SetToolTip(btn_paletteimport, @"Import Chromatics color palette from file");
            tt_mappings.SetToolTip(btn_paletteundo, @"Reload color palette from memory");
            tt_mappings.SetToolTip(cb_palette_categories, @"Filter color mappings");

            InitColorMappingGrid();

            Debug.WriteLine("Hello World");
        }

        private void ToggleMappingControls(bool toggle)
        {
            if (toggle)
            {
                palette_colormanager.ColorEditor.Enabled = true;
                palette_colormanager.ColorGrid.Enabled = true;
                palette_colormanager.ColorWheel.Enabled = true;
                palette_colormanager.ScreenColorPicker.Enabled = true;
            }
            else
            {
                palette_colormanager.ColorEditor.Enabled = false;
                palette_colormanager.ColorGrid.Enabled = false;
                palette_colormanager.ColorWheel.Enabled = false;
                palette_colormanager.ScreenColorPicker.Enabled = false;
            }
        }

        private void InitColorMappingGrid()
        {
            //Enumerate Palette Types to category selection
            for (int i = 0; i <= Enums.Palette.TypeCount; i++)
            {
                var name = EnumExtensions.GetAttribute<DisplayAttribute>((Palette.PaletteTypes)i).Name;
                var item = new ComboboxItem { Value = (Palette.PaletteTypes)i, Text = name };
                cb_palette_categories.Items.Add(item);
            }

            cb_palette_categories.SelectedIndex = 0;
            ToggleMappingControls(false);

            ResetMappingsDataGrid();
        }

        private void ResetMappingsDataGrid()
        {
            if (InvokeRequired)
            {
                ResetMappingsDelegate del = ResetMappingsDataGrid;
                Invoke(del);
            }
            else
            {
                SetupMappingsDataGrid();
            }
        }

        private void SetupMappingsDataGrid()
        {
            /*
            MappingGridStartup = false;
            dG_mappings.AllowUserToAddRows = true;
            dG_mappings.Rows.Clear();

            DrawMappingsDict();
            var i = 0;
            DataGridViewRow[] dgV = new DataGridViewRow[_mappingPalette.Count];

            foreach (var palette in _mappingPalette)
            {
                var paletteItem = (DataGridViewRow)dG_mappings.Rows[0].Clone();
                paletteItem.Cells[dG_mappings.Columns["mappings_col_id"].Index].Value = palette.Key;
                paletteItem.Cells[dG_mappings.Columns["mappings_col_cat"].Index].Value = palette.Value[1];
                paletteItem.Cells[dG_mappings.Columns["mappings_col_type"].Index].Value = palette.Value[0];

                var paletteBtn = new DataGridViewTextBoxCell();
                paletteBtn.Style.BackColor = ColorTranslator.FromHtml(palette.Value[2]);
                paletteBtn.Style.SelectionBackColor = ColorTranslator.FromHtml(palette.Value[2]);

                paletteBtn.Value = "";

                paletteItem.Cells[dG_mappings.Columns["mappings_col_color"].Index] = paletteBtn;

                //dG_mappings.Rows.Add(paletteItem);
                dgV[i] = paletteItem;
                i++;
                paletteBtn.ReadOnly = true;
            }

            dG_mappings.Rows.AddRange(dgV);

            dG_mappings.AllowUserToAddRows = false;
            MappingGridStartup = true;
            */
        }

        private void DrawMappingsDict()
        {
            /*
            foreach (var p in typeof(FfxivColorMappings).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var var = p.Name;
                var color = (string)p.GetValue(ColorMappings);
                var _data = _mappingPalette[var];
                string[] data = { _data[0], _data[1], color, _data[3] };
                _mappingPalette[var] = data;
            }
            */
        }

        private void btn_paletteimport_Click(object sender, EventArgs e)
        {

        }

        private void btn_paletteexport_Click(object sender, EventArgs e)
        {

        }

        private void btn_paletteundo_Click(object sender, EventArgs e)
        {
            /*
            var pmcs = dG_mappings.CurrentRow;
            var pcmsid = (string)pmcs.Cells[dG_mappings.Columns["mappings_col_id"].Index].Value;

            var cm = new FfxivColorMappings();
            var _restore = ColorTranslator.ToHtml(Color.Black);

            foreach (var p in typeof(FfxivColorMappings).GetFields(BindingFlags.Public | BindingFlags.Instance))
                if (p.Name == pcmsid)
                    _restore = (string)p.GetValue(cm);

            var restore = ColorTranslator.FromHtml(_restore);
            palette_colormanager.Color = restore;
            */
        }

        private void cb_palette_categories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MappingGridStartup)
            {
                var filter = (Palette.PaletteTypes)cb_palette_categories.SelectedItem;

                if (filter == Palette.PaletteTypes.All)
                    foreach (DataGridViewRow row in dG_mappings.Rows)
                        row.Visible = true;
                else
                    foreach (DataGridViewRow row in dG_mappings.Rows)
                        if ((Palette.PaletteTypes)row.Cells[dG_mappings.Columns["mappings_col_cat"].Index].Value == filter)
                            row.Visible = true;
                        else
                            row.Visible = false;
            }
        }

        private void dG_mappings_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            e.PaintParts &= ~DataGridViewPaintParts.Focus;
        }

        private void dG_mappings_SelectionChanged(object sender, EventArgs e)
        {
            var color = dG_mappings.CurrentRow.Cells[dG_mappings.Columns["mappings_col_color"].Index].Style.BackColor;
            ToggleMappingControls(true);
            palette_colormanager.Color = color;
            palette_preview.BackColor = color;
        }

    }
}
