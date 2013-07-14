<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VectorTools.aspx.cs" Inherits="Composer.Server.VectorTools
" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="vectorForm" runat="server">
    <table>
        <tr>
            <td>
                <div>
                    <table>
                        <tr>
                            <td>
                                X:
                                <asp:TextBox ID="scaleX" Width="30" runat="server" Text="0"></asp:TextBox>
                            </td>
                            <td>
                                Y:
                                <asp:TextBox ID="scaleY" Width="30" runat="server" Text="0"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:TextBox TextMode=MultiLine Wrap="True" Height="200" Width="400" ID="scaleInput" runat="server"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:TextBox TextMode=MultiLine Wrap="True" Height="200" Width="400" ID="scaleOutput" runat="server"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:Button ID="_ScaleIt" OnClick="ScaleIt" Text="Scale" runat="server"></asp:Button>
                            </td>
                        </tr>
                    </table>
                </div>
            </td>
            <td>
                <div>
                    <table>

                        <tr>
                            <td colspan="2">
                                <asp:TextBox TextMode=MultiLine Wrap="True" Height="200" Width="1000" ID="Before" runat="server"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:TextBox TextMode=MultiLine Wrap="True" Height="200" Width="1000" ID="After" runat="server"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:Button ID="Button1" OnClick="ConvertIt" Text="Path to Canvas" runat="server"></asp:Button>
                            </td>
                        </tr>
                    </table>
                </div>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <div>
                    <table>
                        <tr>
                            <td>
                                X:
                                <asp:TextBox ID="xShift" Width="30" runat="server" Text="0"></asp:TextBox>
                            </td>
                            <td>
                                Y:
                                <asp:TextBox  ID="yShift" Width="30" runat="server" Text="0"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:TextBox TextMode=MultiLine Wrap="True" Height="200" Width="400" ID="InPath" runat="server"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:TextBox TextMode=MultiLine Wrap="True" Height="200" Width="400" ID="OutPath" runat="server"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:Button ID="Button2" OnClick="ShiftIt" Text="Shift" runat="server"></asp:Button>
                            </td>
                        </tr>
                    </table>
                </div>
            </td>
        </tr>
    </table>
    </form>
</body>
</html>
