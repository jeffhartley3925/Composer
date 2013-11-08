using System;
using System.Linq;

namespace Composer.Server
{
    public partial class VectorTools : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        public void ConvertIt(Object sender, EventArgs e)
        {
            string commandScript = Before.Text;
            string[] commandLines = commandScript.Split('\n'); //split canvas commands into array

            for (int m = 0; m < commandLines.Length; m++)
            {
                string commandLine = commandLines[m];
                var commandName = "";
                //remove command name and extraneous punctuation from each command, leaving only parameters, but save
                //the command name so it can be added back later.
                if (commandLine.IndexOf("bezierCurveTo") > 0)
                {
                    commandName = "bezierCurveTo";
                    commandLine = commandLine.Replace("ctx.bezierCurveTo(", "");
                }
                if (commandLine.IndexOf("lineTo") > 0)
                {
                    commandName = "lineTo";
                    commandLine = commandLine.Replace("ctx.lineTo(", "");
                }
                if (commandLine.IndexOf("moveTo") > 0)
                {
                    commandName = "moveTo";
                    commandLine = commandLine.Replace("ctx.moveTo(", "");
                }
                if (commandLine.IndexOf("closePath") > 0)
                {
                    commandName = "closePath";
                    commandLine = commandLine.Replace("ctx.closePath(", "");
                }

                commandLine = commandLine.Replace(")", "");
                commandLine = commandLine.Replace("\r", "");
                commandLine = commandLine.Replace(";", "");
                commandLine = commandLine.Replace(" ", "");

                string[] commandLineParameters = commandLine.Split(','); //split the command parameters (coordinate pairs) so we can operate on them

                //prepend 'x + ' or 'y + ' to each coordinate pair and
                //to that, prepend the command name and add back in extraneous punctuation

                string newCommandLine = string.Format("ctx.{0}(", commandName);
                if (commandName != "closePath")
                {
                    for (int i = 0; i < commandLineParameters.Count(); i++)
                    {
                        string commandLineParameter = commandLineParameters[i];

                        if (i % 2 == 1)
                        {
                            if (!string.IsNullOrEmpty(commandLineParameter))
                            {
                                double param = double.Parse(commandLineParameter);
                                param = Math.Round(param, 2);
                                commandLineParameter = string.Format(", y + {0}", param.ToString());
                                newCommandLine += commandLineParameter;
                            }
                        }
                        else
                        {
                            if (i == 0)
                            {
                                if (!string.IsNullOrEmpty(commandLineParameter))
                                {
                                    double param = double.Parse(commandLineParameter);
                                    param = Math.Round(param, 2);
                                    commandLineParameter = string.Format("x + {0}", param.ToString());
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(commandLineParameter))
                                {
                                    double param = double.Parse(commandLineParameter);
                                    param = Math.Round(param, 2);
                                    commandLineParameter = string.Format(", x + {0}", param.ToString());
                                }
                            }

                            newCommandLine += commandLineParameter;
                        }
                    }
                }
                newCommandLine = string.Format("{0}){1}", newCommandLine, ";\r");
                After.Text += newCommandLine;
            }
        }
        public void ShiftIt(Object sender, EventArgs e)
        {
            double _x, _y;
            int outInt;
            string _coordinate;
            string _coordinates = "";
            double x = double.Parse(xShift.Text);
            double y = double.Parse(yShift.Text);
            string target = InPath.Text;
            string[] coordinates = target.Split(' ');
            string[] scalar;
            foreach (string coordinate in coordinates)
            {
                if (coordinate.IndexOf(",") >= 0)
                {
                    scalar = coordinate.Split(',');
                    string action = scalar[0].Substring(0, 1);
                    if (int.TryParse(action, out outInt))
                    {
                        action = string.Empty;
                        _x = double.Parse(scalar[0]);
                    }
                    else
                    {
                        _x = double.Parse(scalar[0].Substring(1, scalar[0].Length - 1));
                    }
                    _y = double.Parse(scalar[1]);
                    _x = _x + x;
                    _y = _y + y;
                    _coordinate = " " + action + _x.ToString() + "," + _y.ToString();
                    _coordinates = _coordinates + _coordinate;
                }
                else
                {
                    _coordinates = _coordinates + " " + coordinate;
                }
            }
            OutPath.Text = _coordinates;
        }
        public void ScaleIt(Object sender, EventArgs e)
        {
            double _x, _y;
            int outInt;
            string _coordinate;
            string _coordinates = string.Empty;
            double x = double.Parse(scaleX.Text);
            double y = double.Parse(scaleY.Text);
            string target = scaleInput.Text;
            string[] coordinates = target.Split(' ');
            string[] scalar;
            foreach (string coordinate in coordinates)
            {
                if (coordinate.IndexOf(",") >= 0)
                {
                    scalar = coordinate.Split(',');
                    string action = scalar[0].Substring(0, 1);
                    if (int.TryParse(action, out outInt))
                    {
                        action = string.Empty;
                        _x = double.Parse(scalar[0]);
                    }
                    else
                    {
                        _x = double.Parse(scalar[0].Substring(1, scalar[0].Length - 1));
                    }
                    _y = double.Parse(scalar[1]);
                    _x = _x * x;
                    _y = _y * y;
                    _coordinate = " " + action + _x.ToString() + "," + _y.ToString();
                    _coordinates = _coordinates + _coordinate;
                }
                else
                {
                    _coordinates = _coordinates + " " + coordinate;
                }
            }
            scaleOutput.Text = _coordinates;
        }
    }
}