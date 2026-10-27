<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="parcial2.Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Gestión de Encuentro Académico - La universidad de yoiver</title>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
            margin: 0;
            padding: 30px 20px;
            min-height: 100vh;
        }
        .container {
            background: #ffffff;
            padding: 30px 40px;
            border-radius: 12px;
            box-shadow: 0 10px 25px rgba(0, 0, 0, 0.3);
            max-width: 950px;
            margin: 0 auto;
        }
        h2 {
            color: #1e3c72;
            text-align: center;
            margin-bottom: 25px;
            border-bottom: 3px solid #eef2f7;
            padding-bottom: 12px;
        }
        .form-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 15px;
            margin-bottom: 20px;
        }
        .form-group {
            display: flex;
            flex-direction: column;
        }
        label {
            font-weight: 600;
            font-size: 13px;
            color: #444;
            margin-bottom: 5px;
        }
        .textbox, .dropdown {
            padding: 9px 12px;
            border: 1px solid #ced4da;
            border-radius: 6px;
            font-size: 14px;
            background-color: #fdfdfd;
        }
        .textbox:focus, .dropdown:focus {
            border-color: #2a5298;
            outline: none;
            box-shadow: 0 0 6px rgba(42, 82, 152, 0.3);
            background-color: #fff;
        }
        .btn-container {
            grid-column: span 2;
            text-align: center;
            margin-top: 15px;
        }
        .btn {
            background-color: #2a5298;
            color: white;
            border: none;
            padding: 10px 20px;
            font-size: 14px;
            font-weight: 600;
            border-radius: 6px;
            cursor: pointer;
            margin: 0 5px;
            box-shadow: 0 3px 5px rgba(0,0,0,0.1);
        }
        .btn:hover { background-color: #162f5c; }
        .btn-actualizar { background-color: #ffc107; color: #333; }
        .btn-actualizar:hover { background-color: #e0a800; }
        .btn-limpiar { background-color: #6c757d; }
        .btn-limpiar:hover { background-color: #5a6268; }
        .btn-eliminar { background-color: #dc3546; }
        .btn-eliminar:hover { background-color: #bd2130; }
        
        .grid-view {
            width: 100%;
            border-collapse: collapse;
            margin-top: 25px;
        }
        .grid-view th {
            background-color: #2a5298;
            color: white;
            padding: 10px;
            font-size: 13px;
            text-align: left;
        }
        .grid-view td {
            padding: 8px 10px;
            border-bottom: 1px solid #dee2e6;
            font-size: 13px;
            color: #333;
        }
        .grid-view tr:nth-child(even) { background-color: #f8f9fa; }
        .mensaje {
            text-align: center;
            font-weight: bold;
            font-size: 14px;
            margin-top: 15px;
            display: block;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <h2>Sistema de Registro y Gestión - La universidad de yoiver</h2>
            
            <!-- Campos ocultos para almacenar IDs al editar -->
            <asp:HiddenField ID="hfIdRegistro" runat="server" />
            <asp:HiddenField ID="hfIdEstudiante" runat="server" />

            <div class="form-grid">
                <div class="form-group">
                    <label>Nombres:</label>
                    <asp:TextBox ID="txtNombres" runat="server" CssClass="textbox"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Apellidos:</label>
                    <asp:TextBox ID="txtApellidos" runat="server" CssClass="textbox"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Correo:</label>
                    <asp:TextBox ID="txtCorreo" runat="server" CssClass="textbox" TextMode="Email"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Teléfono:</label>
                    <asp:TextBox ID="txtTelefono" runat="server" CssClass="textbox" TextMode="Phone"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Programa:</label>
                    <asp:DropDownList ID="ddlPrograma" runat="server" CssClass="dropdown"></asp:DropDownList>
                </div>
                <div class="form-group">
                    <label>Semestre:</label>
                    <asp:DropDownList ID="ddlSemestre" runat="server" CssClass="dropdown"></asp:DropDownList>
                </div>
                <div class="form-group">
                    <label>Jornada:</label>
                    <asp:DropDownList ID="ddlJornada" runat="server" CssClass="dropdown"></asp:DropDownList>
                </div>
                
                <div class="btn-container">
                    <asp:Button ID="btnRegistrar" runat="server" Text="➕ Registrar Nuevo" OnClick="btnRegistrar_Click" CssClass="btn" />
                    <asp:Button ID="btnActualizar" runat="server" Text="💾 Guardar Cambios" OnClick="btnActualizar_Click" CssClass="btn btn-actualizar" />
                    <asp:Button ID="btnLimpiar" runat="server" Text="🧹 Limpiar Formulario" CausesValidation="false" OnClick="btnLimpiar_Click" CssClass="btn btn-limpiar" />
                </div>
            </div>

            <asp:Label ID="lblMensaje" runat="server" CssClass="mensaje" Text=""></asp:Label>

            <!-- Tabla GridView con el CRUD completo usando la columna 'numero' -->
            <div style="overflow-x: auto;">
                <asp:GridView ID="gvRegistros" runat="server" AutoGenerateColumns="False" CssClass="grid-view" 
                    OnRowCommand="gvRegistros_RowCommand" DataKeyNames="numero,Idestudiante">
                    <Columns>
                        <asp:BoundField DataField="numero" HeaderText="ID" />
                        <asp:BoundField DataField="nombres" HeaderText="Nombres" />
                        <asp:BoundField DataField="apellidos" HeaderText="Apellidos" />
                        <asp:BoundField DataField="email" HeaderText="Correo" />
                        <asp:BoundField DataField="Programa" HeaderText="Programa" />
                        <asp:BoundField DataField="Semestre" HeaderText="Semestre" />
                        <asp:BoundField DataField="Jornada" HeaderText="Jornada" />
                        
                        <asp:TemplateField HeaderText="Acciones">
                            <ItemTemplate>
                                <asp:Button ID="btnSeleccionar" runat="server" Text="✏️ Editar" CommandName="SeleccionarEditar" CommandArgument="<%# Container.DataItemIndex %>" CssClass="btn" style="padding: 5px 9px; font-size: 11px; background-color: #17a2b8;" CausesValidation="false" />
                                <asp:Button ID="btnEliminarFila" runat="server" Text="🗑️ Eliminar" CommandName="EliminarRegistro" CommandArgument="<%# Container.DataItemIndex %>" CssClass="btn btn-eliminar" style="padding: 5px 9px; font-size: 11px;" CausesValidation="false" OnClientClick="return confirm('¿Estás seguro de eliminar este registro?');" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </form>
</body>
</html>