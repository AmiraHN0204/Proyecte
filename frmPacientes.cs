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
    public partial class frmPacientes : Form
    {
        // Nombre correcto — antes decía "Pacientes"
        private const string TABLA = "Paciente";

        Form1 FormularioPrincipal;
        SqlConnection Conexion;

        public frmPacientes(Form1 Formulario)
        {
            InitializeComponent();
            FormularioPrincipal = Formulario;
            Conexion = FormularioPrincipal.ObtenerConexion();
        }

        private void frmPacientes_Load(object sender, EventArgs e)
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

        // Columnas: ID_Paciente (IDENTITY), Nombre_Pac, Direccion_Pac, Telefono_Pac,
        //           CodigoP_Pac, NSS_Pac, Medico_ID_Medico (FK)
        private (object IDPaciente, object NombrePac, object DireccionPac, object TelefonoPac,
                 object CodigoPac, object NSSPac, object MedicoID) LeerCampos()
        {
            return (
                FormularioPrincipal.LeerValorCampo(this, "ID_Paciente", TABLA, FormularioPrincipal.ObtenerTipo("ID_Paciente", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Nombre_Pac", TABLA, FormularioPrincipal.ObtenerTipo("Nombre_Pac", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Direccion_Pac", TABLA, FormularioPrincipal.ObtenerTipo("Direccion_Pac", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Telefono_Pac", TABLA, FormularioPrincipal.ObtenerTipo("Telefono_Pac", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "CodigoP_Pac", TABLA, FormularioPrincipal.ObtenerTipo("CodigoP_Pac", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "NSS_Pac", TABLA, FormularioPrincipal.ObtenerTipo("NSS_Pac", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Medico_ID_Medico", TABLA, FormularioPrincipal.ObtenerTipo("Medico_ID_Medico", TABLA))
            );
        }

        // ── INSERTAR ──────────────────────────────────────────────────────────
        private void BtnInsertar_Click(object sender, EventArgs e)
        {
            var (_, NombrePac, DireccionPac, TelefonoPac, CodigoPac, NSSPac, MedicoID) = LeerCampos();

            if (NombrePac == DBNull.Value || NombrePac == null)
            {
                MessageBox.Show("El Nombre del Paciente es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MedicoID == DBNull.Value || MedicoID == null)
            {
                MessageBox.Show("Seleccione el Médico asignado.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"INSERT INTO Paciente
                               (Nombre_Pac, Direccion_Pac, Telefono_Pac, CodigoP_Pac, NSS_Pac, Medico_ID_Medico)
                           VALUES
                               (@Nombre_Pac, @Direccion_Pac, @Telefono_Pac, @CodigoP_Pac, @NSS_Pac, @Medico_ID_Medico)";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@Nombre_Pac", NombrePac ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Direccion_Pac", DireccionPac ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Telefono_Pac", TelefonoPac ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CodigoP_Pac", CodigoPac ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NSS_Pac", NSSPac ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Medico_ID_Medico", MedicoID);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Paciente registrado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("insertar Paciente", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ELIMINAR ──────────────────────────────────────────────────────────
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            var (IDPaciente, _, _, _, _, _, _) = LeerCampos();

            if (IDPaciente == DBNull.Value || IDPaciente == null)
            {
                MessageBox.Show("Seleccione un paciente del grid para eliminar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"¿Eliminar el Paciente con ID {IDPaciente}?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Paciente WHERE ID_Paciente = @ID_Paciente", Conexion))
                {
                    cmd.Parameters.AddWithValue("@ID_Paciente", IDPaciente);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Paciente eliminado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("eliminar Paciente", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ACTUALIZAR ────────────────────────────────────────────────────────
        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            var (IDPaciente, NombrePac, DireccionPac, TelefonoPac, CodigoPac, NSSPac, MedicoID) = LeerCampos();

            if (IDPaciente == DBNull.Value || IDPaciente == null)
            {
                MessageBox.Show("Seleccione un paciente del grid para actualizar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (NombrePac == DBNull.Value || NombrePac == null)
            {
                MessageBox.Show("El Nombre del Paciente es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"UPDATE Paciente
                           SET Nombre_Pac       = @Nombre_Pac,
                               Direccion_Pac    = @Direccion_Pac,
                               Telefono_Pac     = @Telefono_Pac,
                               CodigoP_Pac      = @CodigoP_Pac,
                               NSS_Pac          = @NSS_Pac,
                               Medico_ID_Medico = @Medico_ID_Medico
                           WHERE ID_Paciente = @ID_Paciente";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@ID_Paciente", IDPaciente);
                    cmd.Parameters.AddWithValue("@Nombre_Pac", NombrePac ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Direccion_Pac", DireccionPac ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Telefono_Pac", TelefonoPac ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CodigoP_Pac", CodigoPac ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NSS_Pac", NSSPac ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Medico_ID_Medico", MedicoID ?? (object)DBNull.Value);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                MessageBox.Show("Paciente actualizado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("actualizar Paciente", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── CONSULTA / FILTRAR ────────────────────────────────────────────────
        private void BtnConsulta_Click(object sender, EventArgs e)
        {
            string termino = MostrarDialogoBusqueda("Buscar en Personal (Nombre, NSS, Población…)");
            if (termino == null) return;   // canceló

            var dgv = this.Controls.Find("dgv" + TABLA, true).FirstOrDefault() as DataGridView;
            if (dgv?.DataSource is DataTable dt)
            {
                if (string.IsNullOrWhiteSpace(termino))
                {
                    dt.DefaultView.RowFilter = "";
                }
                else
                {
                    string t = termino.Replace("'", "''");
                    dt.DefaultView.RowFilter =
                        $"Nombre LIKE '%{t}%' OR NSS LIKE '%{t}%' OR " +
                        $"Poblacion LIKE '%{t}%' OR Provincia LIKE '%{t}%'";
                }
            }
        }
        //Mini dialogo de busqueda
        private string MostrarDialogoBusqueda(string instruccion)
        {
            using (Form dlg = new Form())
            {
                dlg.Text = "Consulta";
                dlg.Size = new Size(380, 140);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false; dlg.MinimizeBox = false;
                dlg.BackColor = Color.FloralWhite;

                Label lbl = new Label { Text = instruccion, Location = new Point(12, 14), AutoSize = true };
                TextBox txt = new TextBox { Location = new Point(12, 38), Width = 340 };
                Button btnOk = new Button
                {
                    Text = "Buscar",
                    DialogResult = DialogResult.OK,
                    Location = new Point(200, 68),
                    Width = 80
                };
                Button btnClear = new Button
                {
                    Text = "Ver todos",
                    DialogResult = DialogResult.No,
                    Location = new Point(290, 68),
                    Width = 70
                };

                dlg.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnClear });
                dlg.AcceptButton = btnOk;

                DialogResult res = dlg.ShowDialog(this);
                if (res == DialogResult.No) return "";          // mostrar todos
                if (res == DialogResult.OK) return txt.Text;
                return null;                                       // canceló
            }
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