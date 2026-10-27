using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace parcial2
{
    public partial class Default : System.Web.UI.Page
    {
        string conexion = ConfigurationManager.ConnectionStrings["conexionDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarListasDesplegables();
                CargarGrillaRegistros();
            }
        }

        private void CargarListasDesplegables()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    con.Open();

                    // 1. Programas (Usamos DISTINCT por si la tabla en la BD tiene duplicados)
                    SqlDataAdapter daProg = new SqlDataAdapter("SELECT DISTINCT Idprograma, nombre FROM Programas", con);
                    DataTable dtProg = new DataTable();
                    daProg.Fill(dtProg);
                    ddlPrograma.DataSource = dtProg;
                    ddlPrograma.DataTextField = "nombre";
                    ddlPrograma.DataValueField = "Idprograma";
                    ddlPrograma.DataBind();
                    ddlPrograma.Items.Insert(0, new ListItem("-- Seleccione Programa --", "0"));

                    // 2. Semestres (Usamos DISTINCT por seguridad)
                    SqlDataAdapter daSem = new SqlDataAdapter("SELECT DISTINCT Idsemestres, descripcion FROM Semestre", con);
                    DataTable dtSem = new DataTable();
                    daSem.Fill(dtSem);
                    ddlSemestre.DataSource = dtSem;
                    ddlSemestre.DataTextField = "descripcion";
                    ddlSemestre.DataValueField = "Idsemestres";
                    ddlSemestre.DataBind();
                    ddlSemestre.Items.Insert(0, new ListItem("-- Seleccione Semestre --", "0"));

                    // 3. Jornadas (Usamos DISTINCT por seguridad)
                    SqlDataAdapter daJor = new SqlDataAdapter("SELECT DISTINCT Idjornada, descripcion FROM Jornada", con);
                    DataTable dtJor = new DataTable();
                    daJor.Fill(dtJor);
                    ddlJornada.DataSource = dtJor;
                    ddlJornada.DataTextField = "descripcion";
                    ddlJornada.DataValueField = "Idjornada";
                    ddlJornada.DataBind();
                    ddlJornada.Items.Insert(0, new ListItem("-- Seleccione Jornada --", "0"));
                }
            }
            catch (Exception ex)
            {
                lblMensaje.ForeColor = System.Drawing.Color.Red;
                lblMensaje.Text = "Error al cargar listas: " + ex.Message;
            }
        }

        private void CargarGrillaRegistros()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    con.Open();
                    string query = @"SELECT r.numero, e.Idestudiante, e.nombres, e.apellidos, e.email, 
                                            p.nombre AS Programa, s.descripcion AS Semestre, j.descripcion AS Jornada
                                     FROM Registro r
                                     INNER JOIN Estudiantes e ON r.Idestudiante = e.Idestudiante
                                     INNER JOIN Programas p ON r.Idprograma = p.Idprograma
                                     INNER JOIN Semestre s ON r.Idsemestres = s.Idsemestres
                                     INNER JOIN Jornada j ON r.Idjornada = j.Idjornada";

                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvRegistros.DataSource = dt;
                    gvRegistros.DataBind();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.ForeColor = System.Drawing.Color.Red;
                lblMensaje.Text = "Error al cargar la tabla: " + ex.Message;
            }
        }

        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNombres.Text) || string.IsNullOrEmpty(txtApellidos.Text) ||
                string.IsNullOrEmpty(txtCorreo.Text) || ddlPrograma.SelectedValue == "0" || ddlSemestre.SelectedValue == "0" || ddlJornada.SelectedValue == "0")
            {
                lblMensaje.ForeColor = System.Drawing.Color.Red;
                lblMensaje.Text = "Por favor, complete todos los campos y seleccione las listas desplegables.";
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    con.Open();

                    // 1. Insertar Estudiante y obtener su ID
                    string queryEstudiante = @"INSERT INTO Estudiantes (nombres, apellidos, email, telefono) 
                                               VALUES (@nombres, @apellidos, @email, @telefono);
                                               SELECT SCOPE_IDENTITY();";

                    int idEstudiante = 0;
                    using (SqlCommand cmdEst = new SqlCommand(queryEstudiante, con))
                    {
                        cmdEst.Parameters.AddWithValue("@nombres", txtNombres.Text.Trim());
                        cmdEst.Parameters.AddWithValue("@apellidos", txtApellidos.Text.Trim());
                        cmdEst.Parameters.AddWithValue("@email", txtCorreo.Text.Trim());
                        cmdEst.Parameters.AddWithValue("@telefono", string.IsNullOrEmpty(txtTelefono.Text) ? (object)DBNull.Value : txtTelefono.Text.Trim());
                        idEstudiante = Convert.ToInt32(cmdEst.ExecuteScalar());
                    }

                    // 2. Insertar Registro Académico
                    string queryRegistro = @"INSERT INTO Registro (Idestudiante, Idprograma, Idsemestres, Idjornada, Idparticipacion, Idtema, comentarios) 
                                             VALUES (@idEstudiante, @idPrograma, @idSemestre, @idJornada, 1, 1, 'Registro web')";

                    using (SqlCommand cmdReg = new SqlCommand(queryRegistro, con))
                    {
                        cmdReg.Parameters.AddWithValue("@idEstudiante", idEstudiante);
                        cmdReg.Parameters.AddWithValue("@idPrograma", ddlPrograma.SelectedValue);
                        cmdReg.Parameters.AddWithValue("@idSemestre", ddlSemestre.SelectedValue);
                        cmdReg.Parameters.AddWithValue("@idJornada", ddlJornada.SelectedValue);
                        cmdReg.ExecuteNonQuery();
                    }

                    lblMensaje.ForeColor = System.Drawing.Color.Green;
                    lblMensaje.Text = "¡Registro guardado exitosamente!";
                    LimpiarFormulario();
                    CargarGrillaRegistros();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.ForeColor = System.Drawing.Color.Red;
                lblMensaje.Text = "Error al registrar: " + ex.Message;
            }
        }

        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hfIdRegistro.Value))
            {
                lblMensaje.ForeColor = System.Drawing.Color.Red;
                lblMensaje.Text = "Seleccione primero un registro de la tabla haciendo clic en 'Editar'.";
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    con.Open();

                    // Actualizar Estudiante
                    string queryEstudiante = "UPDATE Estudiantes SET nombres = @nombres, apellidos = @apellidos, email = @email, telefono = @telefono WHERE Idestudiante = @idEstudiante";
                    SqlCommand cmdEst = new SqlCommand(queryEstudiante, con);
                    cmdEst.Parameters.AddWithValue("@nombres", txtNombres.Text.Trim());
                    cmdEst.Parameters.AddWithValue("@apellidos", txtApellidos.Text.Trim());
                    cmdEst.Parameters.AddWithValue("@email", txtCorreo.Text.Trim());
                    cmdEst.Parameters.AddWithValue("@telefono", string.IsNullOrEmpty(txtTelefono.Text) ? (object)DBNull.Value : txtTelefono.Text.Trim());
                    cmdEst.Parameters.AddWithValue("@idEstudiante", hfIdEstudiante.Value);
                    cmdEst.ExecuteNonQuery();

                    // Actualizar Registro
                    string queryRegistro = "UPDATE Registro SET Idprograma = @idPrograma, Idsemestres = @idSemestre, Idjornada = @idJornada WHERE numero = @idRegistro";
                    SqlCommand cmdReg = new SqlCommand(queryRegistro, con);
                    cmdReg.Parameters.AddWithValue("@idPrograma", ddlPrograma.SelectedValue);
                    cmdReg.Parameters.AddWithValue("@idSemestre", ddlSemestre.SelectedValue);
                    cmdReg.Parameters.AddWithValue("@idJornada", ddlJornada.SelectedValue);
                    cmdReg.Parameters.AddWithValue("@idRegistro", hfIdRegistro.Value);
                    cmdReg.ExecuteNonQuery();

                    lblMensaje.ForeColor = System.Drawing.Color.Green;
                    lblMensaje.Text = "¡Registro actualizado exitosamente!";
                    LimpiarFormulario();
                    CargarGrillaRegistros();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.ForeColor = System.Drawing.Color.Red;
                lblMensaje.Text = "Error al actualizar: " + ex.Message;
            }
        }

        protected void gvRegistros_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "SeleccionarEditar" || e.CommandName == "EliminarRegistro")
            {
                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = gvRegistros.Rows[index];
                int idRegistro = Convert.ToInt32(gvRegistros.DataKeys[index].Values["numero"]);
                int idEstudiante = Convert.ToInt32(gvRegistros.DataKeys[index].Values["Idestudiante"]);

                if (e.CommandName == "SeleccionarEditar")
                {
                    hfIdRegistro.Value = idRegistro.ToString();
                    hfIdEstudiante.Value = idEstudiante.ToString();
                    txtNombres.Text = row.Cells[1].Text;
                    txtApellidos.Text = row.Cells[2].Text;
                    txtCorreo.Text = row.Cells[3].Text;

                    lblMensaje.ForeColor = System.Drawing.Color.Blue;
                    lblMensaje.Text = "Modo edición activado para el registro ID: " + idRegistro + ". Modifique los datos y presione 'Guardar Cambios'.";
                }
                else if (e.CommandName == "EliminarRegistro")
                {
                    EliminarRegistro(idRegistro, idEstudiante);
                }
            }
        }

        private void EliminarRegistro(int idRegistro, int idEstudiante)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    con.Open();

                    // Eliminar Registro primero
                    string queryReg = "DELETE FROM Registro WHERE numero = @idRegistro";
                    SqlCommand cmdReg = new SqlCommand(queryReg, con);
                    cmdReg.Parameters.AddWithValue("@idRegistro", idRegistro);
                    cmdReg.ExecuteNonQuery();

                    // Eliminar Estudiante
                    string queryEst = "DELETE FROM Estudiantes WHERE Idestudiante = @idEstudiante";
                    SqlCommand cmdEst = new SqlCommand(queryEst, con);
                    cmdEst.Parameters.AddWithValue("@idEstudiante", idEstudiante);
                    cmdEst.ExecuteNonQuery();

                    lblMensaje.ForeColor = System.Drawing.Color.Green;
                    lblMensaje.Text = "¡Registro eliminado correctamente!";
                    CargarGrillaRegistros();
                    LimpiarFormulario();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.ForeColor = System.Drawing.Color.Red;
                lblMensaje.Text = "Error al eliminar: " + ex.Message;
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            lblMensaje.Text = "";
        }

        private void LimpiarFormulario()
        {
            hfIdRegistro.Value = "";
            hfIdEstudiante.Value = "";
            txtNombres.Text = "";
            txtApellidos.Text = "";
            txtCorreo.Text = "";
            txtTelefono.Text = "";
            if (ddlPrograma.Items.Count > 0) ddlPrograma.SelectedIndex = 0;
            if (ddlSemestre.Items.Count > 0) ddlSemestre.SelectedIndex = 0;
            if (ddlJornada.Items.Count > 0) ddlJornada.SelectedIndex = 0;
        }
    }
}