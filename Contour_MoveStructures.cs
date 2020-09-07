////////////////////////////////////////////////////////////////////////////////
// script should work with:
//      Eclipse Scripting API
//          15.1.1
//          15.5
//
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Controls;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using System.Drawing;
using System.Windows.Forms;

//[assembly: AssemblyFileVersion("2.1")]
[assembly: AssemblyVersion("1.3")]
[assembly: ESAPIScript(IsWriteable = true)]


namespace VMS.TPS
{
    public class Script
    {
        // Change these IDs to match your clinical conventions
        const string SCRIPT_NAME = "Move-Structure";

        public Script()
        {
        }
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            System.Windows.Forms.Label label = new System.Windows.Forms.Label();
            System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox();
            System.Windows.Forms.Button buttonOk = new System.Windows.Forms.Button();
            System.Windows.Forms.Button buttonCancel = new System.Windows.Forms.Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new System.Drawing.Size(396, 107);
            form.Controls.AddRange(new System.Windows.Forms.Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new System.Drawing.Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
        {
            if (context.Patient == null || context.StructureSet == null)
            {
                System.Windows.MessageBox.Show("Please load a patient, 3D image, and structure set before running this script.", SCRIPT_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            Patient patient = context.Patient;
            patient.BeginModifications();

            StructureSet ss = context.StructureSet;
            Structure targetstructure = SelectStructureWindow.SelectStructure(ss);

            int k = 0;

            // Define offsets
            
            //var inputvalue = Interaction.InputBox("Type Your Value Here");
            double offsetx = 10;
            string value = "10";
            if (InputBox("X-offset", "New X-offset in mm:", ref value) == DialogResult.OK)
            {
                System.Windows.MessageBox.Show(value);
                if (double.TryParse(value, out offsetx))
                {
                    //parsing successful 
                }
                else
                {
                    //parsing failed.                     
                    System.Windows.MessageBox.Show(string.Format("Oops, an invalid value was used for the offset ({0}). The DEFAULT of {1} mm will be used.", value, offsetx));
                }
                System.Windows.MessageBox.Show(offsetx.ToString());
            }
            double offsety = 10;
            double offsetz = 10;
            string newID = "";
            string newIDaddOn = Math.Round(offsetx / 10, 0).ToString() + Math.Round(offsety / 10, 0).ToString() + Math.Round(offsetz / 10, 0).ToString();
            if (targetstructure.Id.Length + newIDaddOn.Length < 14)
            { newID = targetstructure.Id + newIDaddOn; }
            else
            {
                int cut = targetstructure.Id.Length + newIDaddOn.Length - 13;
                newID = targetstructure.Id.Substring(0, targetstructure.Id.Length-cut) + newIDaddOn;
            }
            var newvol = ss.AddStructure("PTV", newID);
            var nPlanes = context.StructureSet.Image.ZSize;

            for (int z = 0; z < nPlanes; z++)
            {
                var contoursOnImapgePlane = targetstructure.GetContoursOnImagePlane(z);


                if (contoursOnImapgePlane != null && contoursOnImapgePlane.Length > 0)
                {
                    foreach (var contour in contoursOnImapgePlane)
                    {
                        VVector[] newcontour = contour;


                        foreach (var pt in contour)
                        {
                            var coordx = pt.x;
                            var coordy = pt.y;
                            var coordz = pt.z;
                            newcontour[k] = new VVector(coordx + offsetx, coordy + offsety, coordz + offsetz);
                            k = k + 1;
                        }

                        newvol.AddContourOnImagePlane(newcontour, z);
                        k = 0;
                    }
                }
            }
        }
        

        class SelectStructureWindow : Window
        {
            public static Structure SelectStructure(StructureSet ss)
            {

                m_w = new Window();
                //m_w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                m_w.WindowStartupLocation = WindowStartupLocation.Manual;
                m_w.Left = 500;
                m_w.Top = 150;
                m_w.Width = 300;
                m_w.Height = 350;
                //m_w.SizeToContent = SizeToContent.Height;
                //m_w.SizeToContent = SizeToContent.Width;
                m_w.Title = "Choose structure to copy and move:";
                var grid = new Grid();
                m_w.Content = grid;
                var list = new System.Windows.Controls.ListBox();
                foreach (var s in ss.Structures.OrderBy(x => x.Id))
                {
                    var tempStruct = s.ToString();
                    if (tempStruct.ToUpper().Contains("PTV") || tempStruct.ToUpper().Contains("ZHK") || tempStruct.ToUpper().Contains("SIB") || tempStruct.ToUpper().Contains("CTV") || tempStruct.ToUpper().Contains("GTV") || tempStruct.ToUpper().StartsWith("Z"))
                    {
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        list.Items.Add(s);
                    }
                }
                list.VerticalAlignment = VerticalAlignment.Top;
                list.Margin = new Thickness(10, 10, 10, 55);
                grid.Children.Add(list);
                var button = new System.Windows.Controls.Button();
                button.Content = "OK";
                button.Height = 40;
                button.VerticalAlignment = VerticalAlignment.Bottom;
                button.Margin = new Thickness(10, 10, 10, 10);
                button.Click += button_Click;
                grid.Children.Add(button);
                if (m_w.ShowDialog() == true)
                {
                    return (Structure)list.SelectedItem;
                }
                return null;
            }

            static Window m_w = null;

            static void button_Click(object sender, RoutedEventArgs e)
            {
                m_w.DialogResult = true;
                m_w.Close();
            }
        }
    }
}
