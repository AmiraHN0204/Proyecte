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
    public partial class frmTitular : Form
    {
        private const string TABLA = "Titular";
        Form1 FormularioPrincipal;
        SqlConnection conexion;
        public frmTitular(Form1 Formulario)
        {
            InitializeComponent();
            FormularioPrincipal = Formulario;
            conexion = FormularioPrincipal.ObtenerConexion();
        }

        private void frmTitular_Load(object sender, EventArgs e)
        {
            this.Tag = TABLA;
            var (btnInsertar, btnEliminar, btnActualizar, btnConsulta, btnLimpiar) =
                FormularioPrincipal.InicializarModulo(this, "Titular");

            btnInsertar.Click += BtnInsertar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnActualizar.Click += BtnActualizar_Click;
            btnConsulta.Click += BtnConsulta_Click;
            btnLimpiar.Click += BtnLimpiar_Click;

            
        }
        private (object IDTitular, object Consultorio) LeerCampos()
        {
            return (
                FormularioPrincipal.LeerValorCampo(this, "Medico_ID_Medico", "Titular", FormularioPrincipal.ObtenerTipo("Medico_ID_Medico", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Consultorio_Principal", "Titular", FormularioPrincipal.ObtenerTipo ("Consultorio_Principal", TABLA))
                );
        }
        private void BtnInsertar_Click(object sender, EventArgs e)
        {
            var(IDTitular, Consultorio) = LeerCampos();

            if (IDTitular == DBNull.Value || Consultorio == null || Consultorio == DBNull.Value) 
            {
                MessageBox.Show("Por favor, complete todos los campos obligatorios.", "Datos Incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            string sql = "INSERT INTO Titular (Medico_ID_Medico, Consultorio_Principal) VALUES (@Medico_ID_Medico, @Consultorio_Principal)";

            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@Medico_ID_Medico", IDTitular);
                    cmd.Parameters.AddWithValue("@Consultorio_Principal", Consultorio);
                    if (conexion.State != ConnectionState.Open) conexion.Open();
                    cmd.ExecuteNonQuery();
                }
                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Titular insertado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al insertar el titular: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conexion.State == ConnectionState.Open) conexion.Close();
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            var (IDTitular, _) = LeerCampos();
            if (IDTitular == DBNull.Value || IDTitular == null)
            {
                MessageBox.Show("Por favor, ingrese el ID del titular a eliminar.", "ID Requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show($"¿Deseas eliminar al titular con ID: {IDTitular}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Titular WHERE Medico_ID_Medico = @Medico_ID_Medico", conexion)) 
                {
                    cmd.Parameters.AddWithValue("@Medico_ID_Medico", IDTitular);
                    if(conexion.State != ConnectionState.Open) conexion.Open();
                    cmd.ExecuteNonQuery();
                }
                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Titular eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar el titular: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conexion.State == ConnectionState.Open) conexion.Close();
            }
        }

        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            var (IDTitular, Consultorio) = LeerCampos();
            if (IDTitular == DBNull.Value || Consultorio == null || Consultorio == DBNull.Value)
            {
                MessageBox.Show("Por favor, complete todos los campos obligatorios para actualizar.", "Datos Incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string sql = "UPDATE Titular SET Consultorio_Principal = @Consultorio_Principal WHERE Medico_ID_Medico = @Medico_ID_Medico";

            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@Medico_ID_Medico", IDTitular);
                    cmd.Parameters.AddWithValue("@Consultorio_Principal", Consultorio ?? DBNull.Value);
                    if(conexion.State != ConnectionState.Open) conexion.Open();
                    cmd.ExecuteNonQuery();
                }
                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Titular actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { FormularioPrincipal.MostrarError("Actualizar Titular", ex); }
            finally
            {
                if (conexion.State == ConnectionState.Open) conexion.Close();
            }
        }

        private void BtnConsulta_Click(object sender, EventArgs e)
        {
            string termino = MostrarDialogoBusqueda("Buscar en Titular (ID de Medico, Consultorio)");
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

                    // Se convierte Medico_ID_Medico a string para poder usar LIKE
                    dt.DefaultView.RowFilter =
                        $"CONVERT(Medico_ID_Medico, 'System.String') LIKE '%{t}%' " +
                        $"OR Consultorio_Principal LIKE '%{t}%'";
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

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            foreach (Control control in this.Controls)
            {
                if (control is TextBox) ((TextBox)control).Clear();
                else if (control is ComboBox) ((ComboBox)control).SelectedIndex = -1;
                else if (control is DateTimePicker) ((DateTimePicker)control).Value = DateTime.Now;
            }
        }
    }
}
