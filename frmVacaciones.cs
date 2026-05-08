using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReVita
{
    public partial class frmVacaciones : Form
    {
        private const string TABLA = "Vacaciones";

        Form1 FormularioPrincipal;
        SqlConnection Conexion;

        public frmVacaciones(Form1 Formulario)
        {
            InitializeComponent();
            FormularioPrincipal = Formulario;
            Conexion = FormularioPrincipal.ObtenerConexion();
        }

        private void frmVacaciones_Load(object sender, EventArgs e)
        {
            this.Tag = TABLA;

            var (btnInsertar, btnEliminar, btnActualizar, btnConsulta, btnLimpiar) =
                FormularioPrincipal.InicializarModulo(this, TABLA);

            btnInsertar.Click += BtnInsertar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnActualizar.Click += BtnActualizar_Click;
            btnConsulta.Click += BtnConsulta_Click;
            btnLimpiar.Click += BtnLimpiar_Click;
        }

        // Columnas: ID_Vacaciones (IDENTITY), Fecha_Inicio (date), Fecha_Fin (date),
        //           Estado (combo TipoVacaciones), Personal_ID_Personal (FK)
        private (object IDVacaciones, object FechaInicio, object FechaFin,
                 object Estado, object PersonalID) LeerCampos()
        {
            return (
                FormularioPrincipal.LeerValorCampo(this, "ID_Vacaciones", TABLA, FormularioPrincipal.ObtenerTipo("ID_Vacaciones", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Fecha_Inicio", TABLA, FormularioPrincipal.ObtenerTipo("Fecha_Inicio", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Fecha_Fin", TABLA, FormularioPrincipal.ObtenerTipo("Fecha_Fin", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Estado", TABLA, FormularioPrincipal.ObtenerTipo("Estado", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Personal_ID_Personal", TABLA, FormularioPrincipal.ObtenerTipo("Personal_ID_Personal", TABLA))
            );
        }

        // ── INSERTAR ──────────────────────────────────────────────────────────
        private void BtnInsertar_Click(object sender, EventArgs e)
        {
            var (_, FechaInicio, FechaFin, Estado, PersonalID) = LeerCampos();

            if (PersonalID == DBNull.Value || PersonalID == null)
            {
                MessageBox.Show("Seleccione el Personal.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (FechaInicio == DBNull.Value || FechaInicio == null ||
                FechaFin == DBNull.Value || FechaFin == null)
            {
                MessageBox.Show("Las fechas de inicio y fin son obligatorias.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validar que Fecha_Fin >= Fecha_Inicio
            if (FechaInicio is DateTime fi && FechaFin is DateTime ff && ff < fi)
            {
                MessageBox.Show("La Fecha Fin no puede ser anterior a la Fecha Inicio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"INSERT INTO Vacaciones (Fecha_Inicio, Fecha_Fin, Estado, Personal_ID_Personal)
                           VALUES (@Fecha_Inicio, @Fecha_Fin, @Estado, @Personal_ID_Personal)";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@Fecha_Inicio", FechaInicio);
                    cmd.Parameters.AddWithValue("@Fecha_Fin", FechaFin);
                    cmd.Parameters.AddWithValue("@Estado", Estado ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Personal_ID_Personal", PersonalID);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Vacaciones registradas correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("insertar Vacaciones", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ELIMINAR ──────────────────────────────────────────────────────────
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            var (IDVacaciones, _, _, _, _) = LeerCampos();

            if (IDVacaciones == DBNull.Value || IDVacaciones == null)
            {
                MessageBox.Show("Seleccione un registro del grid para eliminar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"¿Eliminar las Vacaciones con ID {IDVacaciones}?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Vacaciones WHERE ID_Vacaciones = @ID_Vacaciones", Conexion))
                {
                    cmd.Parameters.AddWithValue("@ID_Vacaciones", IDVacaciones);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Vacaciones eliminadas correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("eliminar Vacaciones", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ACTUALIZAR ────────────────────────────────────────────────────────
        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            var (IDVacaciones, FechaInicio, FechaFin, Estado, PersonalID) = LeerCampos();

            if (IDVacaciones == DBNull.Value || IDVacaciones == null)
            {
                MessageBox.Show("Seleccione un registro del grid para actualizar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (FechaInicio is DateTime fi2 && FechaFin is DateTime ff2 && ff2 < fi2)
            {
                MessageBox.Show("La Fecha Fin no puede ser anterior a la Fecha Inicio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"UPDATE Vacaciones
                           SET Fecha_Inicio        = @Fecha_Inicio,
                               Fecha_Fin           = @Fecha_Fin,
                               Estado              = @Estado,
                               Personal_ID_Personal = @Personal_ID_Personal
                           WHERE ID_Vacaciones = @ID_Vacaciones";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@ID_Vacaciones", IDVacaciones);
                    cmd.Parameters.AddWithValue("@Fecha_Inicio", FechaInicio ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Fecha_Fin", FechaFin ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado", Estado ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Personal_ID_Personal", PersonalID ?? (object)DBNull.Value);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                MessageBox.Show("Vacaciones actualizadas correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("actualizar Vacaciones", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── CONSULTA / FILTRAR ────────────────────────────────────────────────
        private void BtnConsulta_Click(object sender, EventArgs e)
        {
            Panel pnlConsulta = new Panel();
            pnlConsulta.Size = new Size(320, 180);
            pnlConsulta.Location = new Point(
                (this.ClientSize.Width - pnlConsulta.Width) / 2,
                (this.ClientSize.Height - pnlConsulta.Height) / 2);
            pnlConsulta.BorderStyle = BorderStyle.FixedSingle;
            pnlConsulta.BackColor = Color.LightGray;

            System.Windows.Forms.Label lblConsulta = new System.Windows.Forms.Label();
            lblConsulta.Text = "Seleccione un personal para consultar:";
            lblConsulta.Location = new Point(10, 10);
            lblConsulta.AutoSize = true;

            System.Windows.Forms.ComboBox cmbConsultas = new System.Windows.Forms.ComboBox();
            cmbConsultas.Location = new Point(10, 35);
            cmbConsultas.Width = 290;
            cmbConsultas.DropDownStyle = ComboBoxStyle.DropDownList;

            System.Windows.Forms.Button btnEjecutarConsulta = new System.Windows.Forms.Button();
            btnEjecutarConsulta.Text = "Consultar";
            btnEjecutarConsulta.Location = new Point(10, pnlConsulta.Height - btnEjecutarConsulta.Height - 10);

            System.Windows.Forms.Button btnCerrarConsulta = new System.Windows.Forms.Button();
            btnCerrarConsulta.Text = "Cerrar";
            btnCerrarConsulta.Location = new Point(
                pnlConsulta.Width - btnCerrarConsulta.Width - 10,
                pnlConsulta.Height - btnCerrarConsulta.Height - 10);
            btnCerrarConsulta.Click += (s, args) => pnlConsulta.Dispose();

            // Cargar "ID - Nombre" en el ComboBox
            try
            {
                if (Conexion.State != ConnectionState.Open) Conexion.Open();

                SqlCommand Comando = new SqlCommand("SELECT Personal_ID_Personal, Consultorio_Principal FROM Titular", Conexion);
                SqlDataReader reader = Comando.ExecuteReader();

                while (reader.Read())
                {
                    cmbConsultas.Items.Add($"{reader["Medico_ID_Medico"]} - {reader["Consultorio_Principal"]}");
                }
                reader.Close();

                if (cmbConsultas.Items.Count > 0)
                    cmbConsultas.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}");
                return;
            }
            finally
            {
                if (Conexion.State == ConnectionState.Open) Conexion.Close();
            }

            btnEjecutarConsulta.Click += (s, args) =>
            {
                if (cmbConsultas.SelectedItem == null)
                {
                    MessageBox.Show("Seleccione un médico titular para consultar.");
                    return;
                }

                // Extraer solo el ID del texto "ID - Consultorio"
                string IDSeleccionado = cmbConsultas.SelectedItem.ToString().Split('-')[0].Trim();

                string Consulta = "SELECT * FROM Titular WHERE Medico_ID_Medico = @Medico_ID_Medico";
                SqlCommand ComandoConsulta = new SqlCommand(Consulta, Conexion);
                ComandoConsulta.Parameters.AddWithValue("@Medico_ID_Medico", IDSeleccionado);

                try
                {
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    SqlDataReader reader = ComandoConsulta.ExecuteReader();

                    if (reader.Read())
                    {
                        foreach (string campo in FormularioPrincipal.ObtenerCampos(TABLA))
                        {
                            object valor = reader[campo];
                            FormularioPrincipal.EscribirValorCampo(
                                this, campo, TABLA,
                                FormularioPrincipal.ObtenerTipo(campo, TABLA),
                                valor);
                        }
                        pnlConsulta.Dispose();
                    }
                    else
                    {
                        MessageBox.Show("No se encontró el registro.");
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al ejecutar la consulta: {ex.Message}");
                }
                finally
                {
                    if (Conexion.State == ConnectionState.Open) Conexion.Close();
                }
            };

            pnlConsulta.Controls.Add(lblConsulta);
            pnlConsulta.Controls.Add(cmbConsultas);
            pnlConsulta.Controls.Add(btnEjecutarConsulta);
            pnlConsulta.Controls.Add(btnCerrarConsulta);
            this.Controls.Add(pnlConsulta);
            pnlConsulta.BringToFront();
        }

        // ── LIMPIAR ───────────────────────────────────────────────────────────
        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            FormularioPrincipal.LimpiarCampos(TABLA, this);
            var dgv = this.Controls.Find("dgv" + TABLA, true).FirstOrDefault() as DataGridView;
            if (dgv?.DataSource is DataTable dt) dt.DefaultView.RowFilter = "";
        }

        
    }
}