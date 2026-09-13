using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace EcoLogistics.DesktopApp
{
    public class MainForm : Form
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string AUTH_TOKEN = "Bearer test_token_2026";

        // Controles de Encabezado
        private TextBox txtHost = null!;
        private Button btnTestHost = null!;
        private Label lblHostStatus = null!;

        // Tab 1: Facturación SAT (C# :8085)
        private TextBox txtRfcEmisor = null!;
        private TextBox txtRfcReceptor = null!;
        private TextBox txtSubtotal = null!;
        private TextBox txtIva = null!;
        private TextBox txtTotal = null!;
        private TextBox txtConcepto = null!;
        private Button btnTimbrarSoap = null!;
        private RichTextBox rtbFacturaResultado = null!;
        private DataGridView dgvFacturas = null!;
        private Button btnListarFacturasRest = null!;

        // Tab 2: Logística y Envíos (Python :8082)
        private TextBox txtShipOrigen = null!;
        private ComboBox cmbShipDestino = null!;
        private NumericUpDown numShipPeso = null!;
        private ComboBox cmbShipServicio = null!;
        private Button btnCotizarTarifaSoap = null!;
        private RichTextBox rtbTarifaResultado = null!;

        private TextBox txtRastreoGuia = null!;
        private Button btnRastrearRest = null!;
        private RichTextBox rtbRastreoResultado = null!;

        // Tab 3: Monitor de Red
        private RichTextBox rtbLogRed = null!;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "EcoLogistics Desktop Suite - Cliente de Escritorio Heterogéneo (REST & SOAP)";
            this.Size = new Size(1100, 750);
            this.MinimumSize = new Size(950, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // Panel Superior: Configuración de Host
            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(10, 14, 26),
                Padding = new Padding(15, 12, 15, 12)
            };

            Label lblTitle = new Label
            {
                Text = "⚡ EcoLogistics Desktop",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(96, 165, 250),
                AutoSize = true,
                Location = new Point(15, 18)
            };

            Label lblHost = new Label
            {
                Text = "Servidor Base:",
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Location = new Point(270, 22)
            };

            txtHost = new TextBox
            {
                Text = "http://localhost",
                Width = 200,
                Location = new Point(370, 18),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 10f)
            };

            btnTestHost = new Button
            {
                Text = "Probar Conexión",
                Location = new Point(585, 16),
                Width = 130,
                Height = 30,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnTestHost.FlatAppearance.BorderSize = 0;
            btnTestHost.Click += async (s, e) => await TestConnectionAsync();

            lblHostStatus = new Label
            {
                Text = "● Sin probar",
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Location = new Point(730, 22)
            };

            pnlTop.Controls.AddRange(new Control[] { lblTitle, lblHost, txtHost, btnTestHost, lblHostStatus });
            this.Controls.Add(pnlTop);

            // Contenedor de Pestañas
            TabControl tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(15, 8)
            };

            TabPage tabFacturacion = new TabPage("🏛️ Facturación y Timbrado SAT (C# :8085)");
            TabPage tabLogistica = new TabPage("📦 Logística y Paquetería (Python :8082)");
            TabPage tabInspector = new TabPage("🔍 Monitor de Red Distribuida");

            tabFacturacion.BackColor = Color.FromArgb(15, 23, 42);
            tabLogistica.BackColor = Color.FromArgb(15, 23, 42);
            tabInspector.BackColor = Color.FromArgb(15, 23, 42);

            BuildFacturacionTab(tabFacturacion);
            BuildLogisticaTab(tabLogistica);
            BuildInspectorTab(tabInspector);

            tabControl.TabPages.AddRange(new TabPage[] { tabFacturacion, tabLogistica, tabInspector });
            this.Controls.Add(tabControl);
        }

        private void BuildFacturacionTab(TabPage tab)
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            // Grupo Izquierdo: Timbrar Comprobante SOAP
            GroupBox grpSoap = new GroupBox
            {
                Text = " Timbrar Comprobante Fiscal SAT (SOAP CoreWCF :8085) ",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(244, 114, 182),
                Padding = new Padding(12)
            };

            TableLayoutPanel pnlForm = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 260,
                ColumnCount = 2,
                RowCount = 7
            };
            pnlForm.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120f));
            pnlForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            txtRfcEmisor = CreateTextBox("ECO20260101ECO");
            txtRfcReceptor = CreateTextBox("GOLF850315ABC");
            txtSubtotal = CreateTextBox("1899.99");
            txtIva = CreateTextBox("303.99");
            txtTotal = CreateTextBox("2203.98");
            txtConcepto = CreateTextBox("Adquisición de Laptop Dell XPS 15");

            txtSubtotal.TextChanged += (s, e) =>
            {
                if (decimal.TryParse(txtSubtotal.Text, out decimal sub))
                {
                    decimal iva = Math.Round(sub * 0.16m, 2);
                    txtIva.Text = iva.ToString("F2");
                    txtTotal.Text = (sub + iva).ToString("F2");
                }
            };

            AddFormField(pnlForm, 0, "RFC Emisor:", txtRfcEmisor);
            AddFormField(pnlForm, 1, "RFC Receptor:", txtRfcReceptor);
            AddFormField(pnlForm, 2, "Subtotal ($):", txtSubtotal);
            AddFormField(pnlForm, 3, "IVA 16% ($):", txtIva);
            AddFormField(pnlForm, 4, "Total ($):", txtTotal);
            AddFormField(pnlForm, 5, "Concepto:", txtConcepto);

            btnTimbrarSoap = new Button
            {
                Text = "⚡ Timbrar con SOAP CoreWCF",
                Dock = DockStyle.Fill,
                Height = 36,
                BackColor = Color.FromArgb(219, 39, 119),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnTimbrarSoap.FlatAppearance.BorderSize = 0;
            btnTimbrarSoap.Click += async (s, e) => await TimbrarFacturaSoapAsync();
            pnlForm.Controls.Add(btnTimbrarSoap, 1, 6);

            rtbFacturaResultado = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(4, 7, 17),
                ForeColor = Color.FromArgb(147, 197, 253),
                Font = new Font("Consolas", 9f),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };

            grpSoap.Controls.Add(rtbFacturaResultado);
            grpSoap.Controls.Add(pnlForm);

            // Grupo Derecho: Historial de Facturas REST
            GroupBox grpRest = new GroupBox
            {
                Text = " Historial de Facturas Registradas (REST API :8085) ",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(56, 189, 248),
                Padding = new Padding(12)
            };

            Panel pnlTopRest = new Panel { Dock = DockStyle.Top, Height = 45 };
            btnListarFacturasRest = new Button
            {
                Text = "↻ Cargar Facturas (REST :8085)",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(2, 132, 199),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnListarFacturasRest.FlatAppearance.BorderSize = 0;
            btnListarFacturasRest.Click += async (s, e) => await CargarFacturasRestAsync();
            pnlTopRest.Controls.Add(btnListarFacturasRest);

            dgvFacturas = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(4, 7, 17),
                ForeColor = Color.White,
                GridColor = Color.FromArgb(51, 65, 85),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvFacturas.DefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgvFacturas.DefaultCellStyle.ForeColor = Color.White;
            dgvFacturas.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgvFacturas.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(147, 197, 253);
            dgvFacturas.EnableHeadersVisualStyles = false;

            dgvFacturas.Columns.Add("ID", "Folio");
            dgvFacturas.Columns.Add("UUID", "Folio Fiscal UUID");
            dgvFacturas.Columns.Add("Receptor", "RFC Receptor");
            dgvFacturas.Columns.Add("Total", "Total ($)");
            dgvFacturas.Columns.Add("Estatus", "Estatus");

            grpRest.Controls.Add(dgvFacturas);
            grpRest.Controls.Add(pnlTopRest);

            layout.Controls.Add(grpSoap, 0, 0);
            layout.Controls.Add(grpRest, 1, 0);
            tab.Controls.Add(layout);
        }

        private void BuildLogisticaTab(TabPage tab)
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            // Grupo Izquierdo: Cotizador Logístico SOAP (Python :8082)
            GroupBox grpSoap = new GroupBox
            {
                Text = " Cotización de Tarifas de Envío (SOAP WSDL :8082) ",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(244, 114, 182),
                Padding = new Padding(12)
            };

            TableLayoutPanel pnlForm = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 220,
                ColumnCount = 2,
                RowCount = 5
            };
            pnlForm.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120f));
            pnlForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            txtShipOrigen = CreateTextBox("CDMX Centro");
            cmbShipDestino = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White
            };
            cmbShipDestino.Items.AddRange(new object[] { "Guadalajara Jalisco", "Monterrey Nuevo León", "Mérida Yucatán", "Puebla Centro" });
            cmbShipDestino.SelectedIndex = 0;

            numShipPeso = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                DecimalPlaces = 1,
                Minimum = 0.5m,
                Maximum = 100m,
                Value = 3.5m,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White
            };

            cmbShipServicio = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White
            };
            cmbShipServicio.Items.AddRange(new object[] { "EXPRESS", "ESTANDAR", "ECONOMICO" });
            cmbShipServicio.SelectedIndex = 0;

            btnCotizarTarifaSoap = new Button
            {
                Text = "⚡ Calcular Tarifa (SOAP Python)",
                Dock = DockStyle.Fill,
                Height = 36,
                BackColor = Color.FromArgb(219, 39, 119),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnCotizarTarifaSoap.FlatAppearance.BorderSize = 0;
            btnCotizarTarifaSoap.Click += async (s, e) => await CotizarTarifaSoapAsync();

            AddFormField(pnlForm, 0, "Origen:", txtShipOrigen);
            AddFormField(pnlForm, 1, "Destino:", cmbShipDestino);
            AddFormField(pnlForm, 2, "Peso (Kg):", numShipPeso);
            AddFormField(pnlForm, 3, "Servicio:", cmbShipServicio);
            pnlForm.Controls.Add(btnCotizarTarifaSoap, 1, 4);

            rtbTarifaResultado = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(4, 7, 17),
                ForeColor = Color.FromArgb(147, 197, 253),
                Font = new Font("Consolas", 9f),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };

            grpSoap.Controls.Add(rtbTarifaResultado);
            grpSoap.Controls.Add(pnlForm);

            // Grupo Derecho: Rastreo de Guías REST (Python :8082)
            GroupBox grpRest = new GroupBox
            {
                Text = " Rastreo de Paquetería por Guía (REST API :8082) ",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(56, 189, 248),
                Padding = new Padding(12)
            };

            Panel pnlRastreoTop = new Panel { Dock = DockStyle.Top, Height = 80 };
            Label lblGuia = new Label { Text = "Número de Guía:", ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(0, 5), AutoSize = true };
            txtRastreoGuia = CreateTextBox("GUIA-2026-9081");
            txtRastreoGuia.Location = new Point(0, 25);
            txtRastreoGuia.Width = 280;

            btnRastrearRest = new Button
            {
                Text = "🔍 Rastrear Envío (REST)",
                Location = new Point(290, 23),
                Width = 170,
                Height = 30,
                BackColor = Color.FromArgb(2, 132, 199),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnRastrearRest.FlatAppearance.BorderSize = 0;
            btnRastrearRest.Click += async (s, e) => await RastrearGuiaRestAsync();

            pnlRastreoTop.Controls.AddRange(new Control[] { lblGuia, txtRastreoGuia, btnRastrearRest });

            rtbRastreoResultado = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(4, 7, 17),
                ForeColor = Color.FromArgb(52, 211, 153),
                Font = new Font("Consolas", 9.5f),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };

            grpRest.Controls.Add(rtbRastreoResultado);
            grpRest.Controls.Add(pnlRastreoTop);

            layout.Controls.Add(grpSoap, 0, 0);
            layout.Controls.Add(grpRest, 1, 0);
            tab.Controls.Add(layout);
        }

        private void BuildInspectorTab(TabPage tab)
        {
            rtbLogRed = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(4, 7, 17),
                ForeColor = Color.FromArgb(226, 232, 240),
                Font = new Font("Consolas", 9f),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };
            tab.Controls.Add(rtbLogRed);
        }

        private void LogNetwork(string protocol, string method, string url, string? requestBody, string? responseBody, int statusCode, long elapsedMs)
        {
            string log = $"======================================================================\r\n" +
                         $"[{DateTime.Now:HH:mm:ss}] {protocol} - {method} {url}\r\n" +
                         $"Estado: HTTP {statusCode} | Latencia: {elapsedMs} ms\r\n" +
                         $"----------------------------------------------------------------------\r\n" +
                         $"[PETICIÓN SALIENTE]:\r\n{requestBody ?? "(Sin cuerpo)"}\r\n\r\n" +
                         $"[RESPUESTA ENTRANTE]:\r\n{responseBody ?? "(Sin respuesta)"}\r\n\r\n";

            if (rtbLogRed.InvokeRequired)
            {
                rtbLogRed.Invoke(new Action(() => rtbLogRed.AppendText(log)));
            }
            else
            {
                rtbLogRed.AppendText(log);
            }
        }

        private string GetCleanHost(int port, string path)
        {
            string baseHost = txtHost.Text.Trim().TrimEnd('/');
            if (!baseHost.StartsWith("http://") && !baseHost.StartsWith("https://"))
            {
                baseHost = "http://" + baseHost;
            }
            Uri uri = new Uri(baseHost);
            return $"{uri.Scheme}://{uri.Host}:{port}{path}";
        }

        private async Task TestConnectionAsync()
        {
            btnTestHost.Enabled = false;
            lblHostStatus.Text = "● Probando conectividad...";
            lblHostStatus.ForeColor = Color.FromArgb(251, 191, 36);

            try
            {
                string urlCs = GetCleanHost(8085, "/swagger");
                string urlPy = GetCleanHost(8082, "/docs");

                var resp1 = await _httpClient.GetAsync(urlCs);
                var resp2 = await _httpClient.GetAsync(urlPy);

                if (resp1.IsSuccessStatusCode && resp2.IsSuccessStatusCode)
                {
                    lblHostStatus.Text = "● Conectado (C# y Python OK)";
                    lblHostStatus.ForeColor = Color.FromArgb(52, 211, 153);
                }
                else
                {
                    lblHostStatus.Text = $"● Respuesta parcial ({resp1.StatusCode}, {resp2.StatusCode})";
                    lblHostStatus.ForeColor = Color.FromArgb(251, 191, 36);
                }
            }
            catch (Exception ex)
            {
                lblHostStatus.Text = "● Error de conexión";
                lblHostStatus.ForeColor = Color.FromArgb(248, 113, 113);
                MessageBox.Show($"No se pudo conectar a los servicios: {ex.Message}\n\nVerifique la IP configurada o que Docker esté corriendo.", "Error de Red", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTestHost.Enabled = true;
            }
        }

        // 1. Timbrar Comprobante SOAP en C# :8085
        private async Task TimbrarFacturaSoapAsync()
        {
            btnTimbrarSoap.Enabled = false;
            rtbFacturaResultado.Text = "Invocando servicio SOAP CoreWCF en puerto 8085...\n";

            string url = GetCleanHost(8085, "/soap/FacturacionService.svc");
            string soapAction = "http://ecommerce.com/facturacion/IFacturacionService/TimbrarComprobante";

            string soapEnvelope = $@"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Body>
    <TimbrarComprobante xmlns=""http://ecommerce.com/facturacion"">
      <request xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
        <RfcEmisor>{txtRfcEmisor.Text.Trim()}</RfcEmisor>
        <RfcReceptor>{txtRfcReceptor.Text.Trim()}</RfcReceptor>
        <Subtotal>{txtSubtotal.Text.Trim()}</Subtotal>
        <Iva>{txtIva.Text.Trim()}</Iva>
        <Total>{txtTotal.Text.Trim()}</Total>
        <Concepto>{txtConcepto.Text.Trim()}</Concepto>
        <UsuarioAuth>admin</UsuarioAuth>
        <PasswordAuth>admin_pass_2026</PasswordAuth>
      </request>
    </TimbrarComprobante>
  </s:Body>
</s:Envelope>";

            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("SOAPAction", soapAction);
                request.Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");

                var response = await _httpClient.SendAsync(request);
                sw.Stop();
                string responseXml = await response.Content.ReadAsStringAsync();

                LogNetwork("SOAP CoreWCF", "POST", url, soapEnvelope, responseXml, (int)response.StatusCode, sw.ElapsedMilliseconds);

                if (response.IsSuccessStatusCode)
                {
                    XDocument doc = XDocument.Parse(responseXml);
                    XNamespace ns = "http://ecommerce.com/facturacion";
                    var resNode = doc.Descendants(ns + "TimbrarComprobanteResult").FirstOrDefault() 
                                  ?? doc.Descendants("TimbrarComprobanteResult").FirstOrDefault();

                    if (resNode != null)
                    {
                        string uuid = resNode.Element(ns + "FolioFiscalUUID")?.Value ?? resNode.Element("FolioFiscalUUID")?.Value ?? "N/A";
                        string estatus = resNode.Element(ns + "Estatus")?.Value ?? resNode.Element("Estatus")?.Value ?? "N/A";
                        string selloSat = resNode.Element(ns + "SelloDigitalSAT")?.Value ?? resNode.Element("SelloDigitalSAT")?.Value ?? "N/A";
                        string total = resNode.Element(ns + "Total")?.Value ?? resNode.Element("Total")?.Value ?? "N/A";
                        string mensaje = resNode.Element(ns + "Mensaje")?.Value ?? resNode.Element("Mensaje")?.Value ?? "N/A";

                        rtbFacturaResultado.Text = $"✓ COMPROBANTE FISCAL TIMBRADO EXITOSAMENTE (HTTP 200 OK)\n\n" +
                                                  $"Folio Fiscal UUID : {uuid}\n" +
                                                  $"Estatus SAT       : {estatus}\n" +
                                                  $"Total Facturado   : ${total} MXN\n" +
                                                  $"Mensaje           : {mensaje}\n" +
                                                  $"Sello Digital SAT : {selloSat[..Math.Min(35, selloSat.Length)]}...\n\n" +
                                                  $"Tiempo de respuesta: {sw.ElapsedMilliseconds} ms";
                    }
                    else
                    {
                        rtbFacturaResultado.Text = "Respuesta SOAP recibida:\n" + responseXml;
                    }
                }
                else
                {
                    rtbFacturaResultado.Text = $"Error SOAP HTTP {(int)response.StatusCode}:\n" + responseXml;
                }
            }
            catch (Exception ex)
            {
                rtbFacturaResultado.Text = "Error al invocar SOAP C#:\n" + ex.Message;
            }
            finally
            {
                btnTimbrarSoap.Enabled = true;
            }
        }

        // 2. Cargar Facturas REST en C# :8085
        private async Task CargarFacturasRestAsync()
        {
            btnListarFacturasRest.Enabled = false;
            string url = GetCleanHost(8085, "/api/v1/facturas");
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", AUTH_TOKEN);

                var response = await _httpClient.SendAsync(request);
                sw.Stop();
                string responseJson = await response.Content.ReadAsStringAsync();

                LogNetwork("REST (JSON)", "GET", url, null, responseJson, (int)response.StatusCode, sw.ElapsedMilliseconds);

                if (response.IsSuccessStatusCode)
                {
                    using JsonDocument doc = JsonDocument.Parse(responseJson);
                    dgvFacturas.Rows.Clear();

                    foreach (var item in doc.RootElement.EnumerateArray())
                    {
                        string id = item.GetProperty("id").GetString() ?? "";
                        string uuid = item.GetProperty("folio_fiscal_uuid").GetString() ?? "";
                        string receptor = item.GetProperty("rfc_receptor").GetString() ?? "";
                        decimal total = item.GetProperty("total").GetDecimal();
                        string estatus = item.GetProperty("estatus").GetString() ?? "";

                        dgvFacturas.Rows.Add(id, uuid, receptor, $"${total:F2}", estatus);
                    }
                }
                else
                {
                    MessageBox.Show($"Error HTTP {(int)response.StatusCode}: {responseJson}", "Error REST", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar facturas REST: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnListarFacturasRest.Enabled = true;
            }
        }

        // 3. Cotizar Tarifa SOAP en Python :8082
        private async Task CotizarTarifaSoapAsync()
        {
            btnCotizarTarifaSoap.Enabled = false;
            rtbTarifaResultado.Text = "Invocando SOAP CalcularTarifa en Python (:8082)...\n";

            string url = GetCleanHost(8082, "/soap/envios");
            string soapAction = "http://ecommerce.com/envios#CalcularTarifa";

            string soapEnvelope = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:env=""http://ecommerce.com/envios"">
  <soapenv:Header/>
  <soapenv:Body>
    <env:CalcularTarifa>
      <env:origen>{txtShipOrigen.Text.Trim()}</env:origen>
      <env:destino>{cmbShipDestino.SelectedItem}</env:destino>
      <env:peso_kg>{numShipPeso.Value}</env:peso_kg>
      <env:tipo_servicio>{cmbShipServicio.SelectedItem}</env:tipo_servicio>
    </env:CalcularTarifa>
  </soapenv:Body>
</soapenv:Envelope>";

            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("SOAPAction", soapAction);
                request.Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");

                var response = await _httpClient.SendAsync(request);
                sw.Stop();
                string responseXml = await response.Content.ReadAsStringAsync();

                LogNetwork("SOAP WSDL", "POST", url, soapEnvelope, responseXml, (int)response.StatusCode, sw.ElapsedMilliseconds);

                if (response.IsSuccessStatusCode)
                {
                    XDocument doc = XDocument.Parse(responseXml);
                    string costo = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "costo_envio")?.Value ?? "0";
                    string dias = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "tiempo_estimado_dias")?.Value ?? "0";
                    string dist = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "distancia_aprox_km")?.Value ?? "0";
                    string moneda = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "moneda")?.Value ?? "MXN";

                    rtbTarifaResultado.Text = $"✓ TARIFA COTIZADA MEDIANTE SOAP PYTHON (HTTP 200 OK)\n\n" +
                                              $"Costo de Envío   : ${costo} {moneda}\n" +
                                              $"Tiempo Estimado  : {dias} día(s) hábiles\n" +
                                              $"Distancia Aprox. : {dist} Km\n" +
                                              $"Modalidad        : {cmbShipServicio.SelectedItem}\n" +
                                              $"Destino          : {cmbShipDestino.SelectedItem}\n\n" +
                                              $"Latencia SOAP: {sw.ElapsedMilliseconds} ms";
                }
                else
                {
                    rtbTarifaResultado.Text = $"Fallo SOAP HTTP {(int)response.StatusCode}:\n" + responseXml;
                }
            }
            catch (Exception ex)
            {
                rtbTarifaResultado.Text = "Error SOAP Python: " + ex.Message;
            }
            finally
            {
                btnCotizarTarifaSoap.Enabled = true;
            }
        }

        // 4. Rastrear Guía REST en Python :8082
        private async Task RastrearGuiaRestAsync()
        {
            string guia = txtRastreoGuia.Text.Trim();
            if (string.IsNullOrEmpty(guia)) return;

            btnRastrearRest.Enabled = false;
            string url = GetCleanHost(8082, $"/api/v1/envios/{guia}");
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", AUTH_TOKEN);

                var response = await _httpClient.SendAsync(request);
                sw.Stop();
                string responseJson = await response.Content.ReadAsStringAsync();

                LogNetwork("REST (JSON)", "GET", url, null, responseJson, (int)response.StatusCode, sw.ElapsedMilliseconds);

                if (response.IsSuccessStatusCode)
                {
                    using JsonDocument doc = JsonDocument.Parse(responseJson);
                    var root = doc.RootElement;
                    string numGuia = root.GetProperty("numero_guia").GetString() ?? "";
                    string estado = root.GetProperty("estado").GetString() ?? "";
                    string origen = root.GetProperty("origen").GetString() ?? "";
                    string destino = root.GetProperty("destino").GetString() ?? "";
                    string fecha = root.GetProperty("fecha_entrega_estimada").GetString() ?? "";

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"✓ GUÍA LOGÍSTICA ENCONTRADA (HTTP 200 OK)");
                    sb.AppendLine($"Guía             : {numGuia}");
                    sb.AppendLine($"Estado Logístico : {estado}");
                    sb.AppendLine($"Ruta             : {origen} -> {destino}");
                    sb.AppendLine($"Fecha Estimada   : {fecha}\n");
                    sb.AppendLine("Historial de Eventos:");

                    if (root.TryGetProperty("historial_eventos", out var hist) && hist.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var ev in hist.EnumerateArray())
                        {
                            string ts = ev.GetProperty("timestamp").GetString() ?? "";
                            string st = ev.GetProperty("estado").GetString() ?? "";
                            string ub = ev.GetProperty("ubicacion").GetString() ?? "";
                            string com = ev.GetProperty("comentario").GetString() ?? "";
                            sb.AppendLine($" • [{ts}] {st} @ {ub} ({com})");
                        }
                    }

                    rtbRastreoResultado.Text = sb.ToString();
                }
                else
                {
                    rtbRastreoResultado.Text = $"Guía '{guia}' no localizada en el sistema (HTTP {(int)response.StatusCode}).";
                }
            }
            catch (Exception ex)
            {
                rtbRastreoResultado.Text = "Error REST Python: " + ex.Message;
            }
            finally
            {
                btnRastrearRest.Enabled = true;
            }
        }

        private TextBox CreateTextBox(string defaultVal)
        {
            return new TextBox
            {
                Text = defaultVal,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private void AddFormField(TableLayoutPanel panel, int row, string labelText, Control inputControl)
        {
            Label lbl = new Label
            {
                Text = labelText,
                ForeColor = Color.FromArgb(148, 163, 184),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };
            panel.Controls.Add(lbl, 0, row);
            panel.Controls.Add(inputControl, 1, row);
        }
    }
}
