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
    public partial class frmSustituto : Form
    {
        public const string TABLA = "Sustituto";

        Form1 FormularioPrincipal;
        SqlConnection Conexion;
        public frmSustituto(Form1 Formulario)
        {
            InitializeComponent();
            FormularioPrincipal = Formulario;
            Conexion = FormularioPrincipal.ObtenerConexion();
        }

        private void frmSustituto_Load(object sender, EventArgs e)
        {
            this.Tag = TABLA;

            var (btnInsertar, btnEliminar, btnActualizar, btnConsulta, btnLimpiar) =
                FormularioPrincipal.InicializarModulo(this, "Sustituto");

            btnInsertar.Click += BtnInsertar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnActualizar.Visible = false;
            btnConsulta.Visible = false;
            btnLimpiar.Visible = false;
        }

        private object LeerCampos()
        {
            return (
                FormularioPrincipal.LeerValorCampo(this, "Medico_ID_Medico", "Sustituto", FormularioPrincipal.ObtenerTipo("Medico_ID_Medico", "Sustituto"))
            );
        }

        private void BtnInsertar_Click(object sender, EventArgs e)
        {
            var IDSustituto = LeerCampos();
            if (IDSustituto == null) 
            {
                MessageBox.Show("Seleccione el Médico.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string sql = @"INSERT INTO Sustituto (Medico_ID_Medico) VALUES (@Medico_ID_Medico)";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion)) 
                {
                    cmd.Parameters.AddWithValue("@Medico_ID_Medico", IDSustituto);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Sustituto insertado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) 
            {
                FormularioPrincipal.MostrarError("insertar Sustituto", ex);
            }
            finally
            {
                if (Conexion.State == ConnectionState.Open) Conexion.Close();
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            var IDSustituto = LeerCampos();
            if (IDSustituto == DBNull.Value || IDSustituto == null) 
            {
                MessageBox.Show("Seleccione el Médico.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show($"¿Confirma que desea eliminar el sustituto con ID: {IDSustituto}?", "Confirmación",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string sql = @"DELETE FROM Sustituto WHERE Medico_ID_Medico = @Medico_ID_Medico";
                try
                {
                    using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                    {
                        cmd.Parameters.AddWithValue("@Medico_ID_Medico", IDSustituto);
                        if (Conexion.State != ConnectionState.Open) Conexion.Open();
                        cmd.ExecuteNonQuery();
                    }
                    FormularioPrincipal.CargarDatos(TABLA, this);
                    FormularioPrincipal.LimpiarCampos(TABLA, this);
                    MessageBox.Show("Sustituto eliminado correctamente.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    FormularioPrincipal.MostrarError("eliminar Sustituto", ex);
                }
                finally
                {
                    if (Conexion.State == ConnectionState.Open) Conexion.Close();
                }
            }
        }

        private void BtnConsulta_Click(object sender, EventArgs e)
        {
        }

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
        }
    }
}
