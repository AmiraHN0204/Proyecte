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
// HOLA AMIGOS DE YOUTUBE ESTAMOS AQUI EN UN NUEVO VIDEO DE REPARACION DE CODIGO EN VISUAL STUDIO, EN ESTE CASO TENEMOS EL FORMULARIO DE INTERINO, QUE ES UNA TABLA QUE RELACIONA A LOS MEDICOS CON SUS CONTRATOS INTERINOS, ASI QUE VAMOS A VER COMO FUNCIONA ESTE
// FORMULARIO Y COMO SE REALIZAN LAS OPERACIONES BASICAS DE CRUD (CREAR, LEER, ACTUALIZAR Y ELIMINAR) EN ESTA TABLA. ASI QUE SIN MAS PREAMBULOS, VAMOS A EMPEZAR CON EL CODIGO.
namespace ReVita
{
    public partial class frmInterino : Form
    {
        private const string TABLA = "Interino";

        Form1 FormularioPrincipal;
        SqlConnection Conexion;

        public frmInterino(Form1 Formulario)
        {
            InitializeComponent();
            FormularioPrincipal = Formulario;
            Conexion = FormularioPrincipal.ObtenerConexion();
        }

        private void frmInterino_Load(object sender, EventArgs e)
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

        // Columnas: Medico_ID_Medico (PK + FK → Medico), Fecha_FinContrato (date)
        private (object MedicoID, object FechaFin) LeerCampos()
        {
            return (
                FormularioPrincipal.LeerValorCampo(this, "Medico_ID_Medico", TABLA, FormularioPrincipal.ObtenerTipo("Medico_ID_Medico", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Fecha_FinContrato", TABLA, FormularioPrincipal.ObtenerTipo("Fecha_FinContrato", TABLA))
            );
        }

        // ── INSERTAR ──────────────────────────────────────────────────────────
        private void BtnInsertar_Click(object sender, EventArgs e)
        {
            var (MedicoID, FechaFin) = LeerCampos();

            if (MedicoID == DBNull.Value || MedicoID == null)
            {
                MessageBox.Show("Seleccione el Médico.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"INSERT INTO Interino (Medico_ID_Medico, Fecha_FinContrato)
                           VALUES (@Medico_ID_Medico, @Fecha_FinContrato)";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@Medico_ID_Medico", MedicoID);
                    cmd.Parameters.AddWithValue("@Fecha_FinContrato", FechaFin ?? DBNull.Value);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Interino registrado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("insertar Interino", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ELIMINAR ──────────────────────────────────────────────────────────
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            var (MedicoID, _) = LeerCampos();

            if (MedicoID == DBNull.Value || MedicoID == null)
            {
                MessageBox.Show("Seleccione un interino del grid para eliminar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"¿Eliminar el Interino del Médico {MedicoID}?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Interino WHERE Medico_ID_Medico = @Medico_ID_Medico", Conexion))
                {
                    cmd.Parameters.AddWithValue("@Medico_ID_Medico", MedicoID);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Interino eliminado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("eliminar Interino", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ACTUALIZAR (solo Fecha_FinContrato) ───────────────────────────────
        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            var (MedicoID, FechaFin) = LeerCampos();

            if (MedicoID == DBNull.Value || MedicoID == null)
            {
                MessageBox.Show("Seleccione un interino del grid para actualizar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"UPDATE Interino
                           SET Fecha_FinContrato = @Fecha_FinContrato
                           WHERE Medico_ID_Medico = @Medico_ID_Medico";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@Medico_ID_Medico", MedicoID);
                    cmd.Parameters.AddWithValue("@Fecha_FinContrato", FechaFin ?? DBNull.Value);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                MessageBox.Show("Interino actualizado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("actualizar Interino", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── CONSULTA ─────────────────────────────────────────────────────────
        private void BtnConsulta_Click(object sender, EventArgs e)
        {
            // Recarga mostrando todos (útil para quitar filtros)
            FormularioPrincipal.CargarDatos(TABLA, this);

            string termino = MostrarDialogoBusqueda("Buscar en Interino (ID de Medico, Fecha de Fin de Contrato)");
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

                    // Se convierten ambas columnas a string para poder usar LIKE
                    dt.DefaultView.RowFilter =
                        $"CONVERT(Medico_ID_Medico, 'System.String') LIKE '%{t}%' " +
                        $"OR CONVERT(Fecha_FinContrato, 'System.String') LIKE '%{t}%'";
                }
            }
        }

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
        private void BtnLimpiar_Click(object sender, EventArgs e) =>
            FormularioPrincipal.LimpiarCampos(TABLA, this);
    }
}
