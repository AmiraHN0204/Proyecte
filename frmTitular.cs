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
                if (conexion.State != ConnectionState.Open) conexion.Open();

                SqlCommand Comando = new SqlCommand("SELECT Medico_ID_Medico, Consultorio_Principal FROM Titular", conexion);
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
                if (conexion.State == ConnectionState.Open) conexion.Close();
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
                SqlCommand ComandoConsulta = new SqlCommand(Consulta, conexion);
                ComandoConsulta.Parameters.AddWithValue("@Medico_ID_Medico", IDSeleccionado);

                try
                {
                    if (conexion.State != ConnectionState.Open) conexion.Open();
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
                    if (conexion.State == ConnectionState.Open) conexion.Close();
                }
            };

            pnlConsulta.Controls.Add(lblConsulta);
            pnlConsulta.Controls.Add(cmbConsultas);
            pnlConsulta.Controls.Add(btnEjecutarConsulta);
            pnlConsulta.Controls.Add(btnCerrarConsulta);
            this.Controls.Add(pnlConsulta);
            pnlConsulta.BringToFront();
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
