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
        private Button btnImportarOrdenPhp = null!;
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
        private Button btnCargarUltimaGuia = null!;
        private Button btnAvanzarEstado = null!;
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
            this.Size = new Size(1150, 780);
            this.MinimumSize = new Size(1000, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // 1. Panel Superior: Configuración de Host
            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(10, 14, 26),
                Padding = new Padding(16, 12, 16, 12)
            };

            Label lblTitle = new Label
            {
                Text = "⚡ EcoLogistics Desktop",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(96, 165, 250),
                AutoSize = true,
                Location = new Point(16, 16)
            };

            Label lblHost = new Label
            {
                Text = "Servidor Base:",
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Location = new Point(270, 20)
            };

            txtHost = new TextBox
            {
                Text = "http://localhost",
                Width = 210,
                Location = new Point(370, 16),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 10f)
            };

            btnTestHost = new Button
            {
                Text = "Probar Conexión",
                Location = new Point(595, 14),
                Width = 135,
                Height = 32,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btnTestHost.FlatAppearance.BorderSize = 0;
            btnTestHost.Click += async (s, e) => await TestConnectionAsync();

            lblHostStatus = new Label
            {
                Text = "● Sin probar",
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Location = new Point(745, 20)
            };

            pnlTop.Controls.AddRange(new Control[] { lblTitle, lblHost, txtHost, btnTestHost, lblHostStatus });

            // 2. Contenedor de Pestañas
            TabControl tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Appearance = TabAppearance.FlatButtons,
                ItemSize = new Size(0, 1),
                SizeMode = TabSizeMode.Fixed
            };

            TabPage tabFacturacion = new TabPage();
            TabPage tabLogistica = new TabPage();
            TabPage tabInspector = new TabPage();

            tabFacturacion.BackColor = Color.FromArgb(15, 23, 42);
            tabLogistica.BackColor = Color.FromArgb(15, 23, 42);
            tabInspector.BackColor = Color.FromArgb(15, 23, 42);

            BuildFacturacionTab(tabFacturacion);
            BuildLogisticaTab(tabLogistica);
            BuildInspectorTab(tabInspector);

            tabControl.TabPages.AddRange(new TabPage[] { tabFacturacion, tabLogistica, tabInspector });

            // 3. Barra de Navegación Moderna de Pestañas (Resuelve superposición y dibujo en WinForms)
            Panel pnlNavBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(12, 6, 12, 6)
            };

            FlowLayoutPanel flowTabs = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false
            };

            Button btnNavFacturacion = CreateNavButton("🏛️ Facturación SAT (C# :8085)", true);
            Button btnNavLogistica = CreateNavButton("📦 Logística y Envíos (Python :8082)", false);
            Button btnNavInspector = CreateNavButton("🔍 Monitor de Red Distribuida", false);

            Button[] navButtons = new Button[] { btnNavFacturacion, btnNavLogistica, btnNavInspector };

            btnNavFacturacion.Click += (s, e) => SetActiveTab(0, tabControl, navButtons);
            btnNavLogistica.Click += (s, e) => SetActiveTab(1, tabControl, navButtons);
            btnNavInspector.Click += (s, e) => SetActiveTab(2, tabControl, navButtons);

            flowTabs.Controls.AddRange(navButtons);
            pnlNavBar.Controls.Add(flowTabs);

            // Orden estricto de adición a Controls para respetar z-order de WinForms Docking
            this.Controls.Add(tabControl);
            this.Controls.Add(pnlNavBar);
            this.Controls.Add(pnlTop);
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
                Height = 330,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(0, 4, 0, 8)
            };
            pnlForm.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115f));
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

            btnImportarOrdenPhp = new Button
            {
                Text = "📥 Cargar Última Orden Web (PHP :8083)",
                Dock = DockStyle.Fill,
                Height = 32,
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btnImportarOrdenPhp.FlatAppearance.BorderSize = 0;
            btnImportarOrdenPhp.Click += async (s, e) => await ImportarOrdenPhpAsync();
            pnlForm.Controls.Add(btnImportarOrdenPhp, 1, 6);

            btnTimbrarSoap = new Button
            {
                Text = "⚡ Timbrar con SOAP (:8085)",
                Dock = DockStyle.Fill,
                Height = 34,
                BackColor = Color.FromArgb(219, 39, 119),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btnTimbrarSoap.FlatAppearance.BorderSize = 0;
            btnTimbrarSoap.Click += async (s, e) => await TimbrarFacturaSoapAsync();
            pnlForm.Controls.Add(btnTimbrarSoap, 1, 7);

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

            Panel pnlRastreoTop = new Panel { Dock = DockStyle.Top, Height = 105 };
            Label lblGuia = new Label { Text = "Número de Guía:", ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(0, 4), AutoSize = true };
            txtRastreoGuia = CreateTextBox("GUIA-2026-9081");
            txtRastreoGuia.Location = new Point(0, 24);
            txtRastreoGuia.Width = 220;

            btnRastrearRest = new Button
            {
                Text = "🔍 Rastrear",
                Location = new Point(226, 23),
                Width = 110,
                Height = 28,
                BackColor = Color.FromArgb(2, 132, 199),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btnRastrearRest.FlatAppearance.BorderSize = 0;
            btnRastrearRest.Click += async (s, e) => await RastrearGuiaRestAsync();

            btnCargarUltimaGuia = new Button
            {
                Text = "📥 Cargar Guía Web",
                Location = new Point(342, 23),
                Width = 150,
                Height = 28,
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btnCargarUltimaGuia.FlatAppearance.BorderSize = 0;
            btnCargarUltimaGuia.Click += async (s, e) => await CargarUltimaGuiaAsync();

            btnAvanzarEstado = new Button
            {
                Text = "🚚 Avanzar Estado de Entrega (PUT :8082)",
                Location = new Point(0, 60),
                Width = 320,
                Height = 32,
                BackColor = Color.FromArgb(217, 119, 6),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnAvanzarEstado.FlatAppearance.BorderSize = 0;
            btnAvanzarEstado.Click += async (s, e) => await AvanzarEstadoEnvioAsync();

            pnlRastreoTop.Controls.AddRange(new Control[] { lblGuia, txtRastreoGuia, btnRastrearRest, btnCargarUltimaGuia, btnAvanzarEstado });

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
                        string id = GetJsonString(item, "folio", "id", "Folio", "Id");
                        string uuid = GetJsonString(item, "folioFiscalUUID", "folio_fiscal_uuid", "FolioFiscalUUID", "uuid", "UUID");
                        string receptor = GetJsonString(item, "rfcCliente", "rfc_receptor", "RfcCliente", "RfcReceptor");
                        decimal total = GetJsonDecimal(item, "total", "Total");
                        string estatus = GetJsonString(item, "estatus", "Estatus");

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

        // 2b. Importar Última Orden Creada en PHP :8083 (Web App)
        private async Task ImportarOrdenPhpAsync()
        {
            btnImportarOrdenPhp.Enabled = false;
            string url = GetCleanHost(8083, "/api/v1/ordenes");
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
                    bool found = false;
                    foreach (var item in doc.RootElement.EnumerateArray())
                    {
                        string orderId = GetJsonString(item, "id", "Id", "folio");
                        string cliente = GetJsonString(item, "cliente_nombre", "clienteNombre", "ClienteNombre");
                        decimal subtotal = GetJsonDecimal(item, "subtotal", "Subtotal");
                        decimal impuestos = GetJsonDecimal(item, "impuestos", "Impuestos");
                        decimal total = GetJsonDecimal(item, "total", "Total");

                        txtConcepto.Text = $"Facturación de {orderId} ({cliente})";
                        txtSubtotal.Text = subtotal.ToString("F2");
                        txtIva.Text = impuestos.ToString("F2");
                        txtTotal.Text = total.ToString("F2");

                        rtbFacturaResultado.Text = $"[OK] ¡Orden '{orderId}' importada de PHP (:8083) exitosamente!\n\n" +
                                                   $"• Cliente     : {cliente}\n" +
                                                   $"• Subtotal    : ${subtotal:F2} USD\n" +
                                                   $"• IVA 16%     : ${impuestos:F2} USD\n" +
                                                   $"• Total Orden : ${total:F2} USD\n\n" +
                                                   "Haga clic en '⚡ Timbrar con SOAP (:8085)' para generar el comprobante fiscal ante el SAT en C# (:8085).";
                        found = true;
                        break;
                    }

                    if (!found)
                    {
                        MessageBox.Show("No se encontraron órdenes registradas en PHP (:8083).\nGenere una orden de compra desde la Web App primero.", "Sin Órdenes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show($"Error HTTP {(int)response.StatusCode} al consultar órdenes en PHP: {responseJson}", "Error REST", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con el microservicio PHP (:8083): " + ex.Message, "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnImportarOrdenPhp.Enabled = true;
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
                    string numGuia = GetJsonString(root, "numero_guia", "numeroGuia", "NumeroGuia", "guia");
                    string estado = GetJsonString(root, "estado", "Estado");
                    string origen = GetJsonString(root, "origen", "remitente_direccion", "remitente_nombre", "Origen");
                    string destino = GetJsonString(root, "destino", "destinatario_direccion", "destinatario_nombre", "Destino");
                    string fecha = GetJsonString(root, "fecha_entrega_estimada", "fechaEntregaEstimada", "FechaEntregaEstimada");

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
                            string ts = GetJsonString(ev, "timestamp", "Timestamp");
                            string st = GetJsonString(ev, "estado", "Estado");
                            string ub = GetJsonString(ev, "ubicacion", "Ubicacion");
                            string com = GetJsonString(ev, "descripcion", "comentario", "Descripcion", "Comentario");
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

        // 4b. Cargar Última Guía Generada en la Web (Python :8082)
        private async Task CargarUltimaGuiaAsync()
        {
            btnCargarUltimaGuia.Enabled = false;
            string url = GetCleanHost(8082, "/api/v1/envios");
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
                    string? ultimaGuia = null;
                    string? ultimoDestino = null;
                    string? ultimoEstado = null;

                    foreach (var item in doc.RootElement.EnumerateArray())
                    {
                        ultimaGuia = GetJsonString(item, "numero_guia", "numeroGuia", "guia");
                        ultimoDestino = GetJsonString(item, "destinatario_nombre", "destinatario", "Destino");
                        ultimoEstado = GetJsonString(item, "estado", "Estado");
                    }

                    if (!string.IsNullOrEmpty(ultimaGuia))
                    {
                        txtRastreoGuia.Text = ultimaGuia;
                        rtbRastreoResultado.Text = $"[OK] ¡Última guía cargada: {ultimaGuia}!\n" +
                                                  $"Destinatario : {ultimoDestino}\n" +
                                                  $"Estado actual: {ultimoEstado}\n\n" +
                                                  "Consultando bitácora completa...";
                        await RastrearGuiaRestAsync();
                    }
                    else
                    {
                        MessageBox.Show("No se encontraron envíos registrados en Python (:8082).\nCree un pedido en la Web App para que se genere su guía automáticamente.", "Sin Envíos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show($"Error HTTP {(int)response.StatusCode} al consultar envíos en Python: {responseJson}", "Error REST", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con el microservicio Python (:8082): " + ex.Message, "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCargarUltimaGuia.Enabled = true;
            }
        }

        // 4c. Avanzar Estado Logístico del Paquete (PUT Python :8082)
        private async Task AvanzarEstadoEnvioAsync()
        {
            string guia = txtRastreoGuia.Text.Trim();
            if (string.IsNullOrEmpty(guia))
            {
                MessageBox.Show("Ingrese un número de guía o presione '📥 Cargar Guía Web' primero.", "Guía Requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnAvanzarEstado.Enabled = false;
            string getUrl = GetCleanHost(8082, $"/api/v1/envios/{guia}");
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                using var getReq = new HttpRequestMessage(HttpMethod.Get, getUrl);
                getReq.Headers.Add("Authorization", AUTH_TOKEN);
                var getResp = await _httpClient.SendAsync(getReq);

                if (!getResp.IsSuccessStatusCode)
                {
                    MessageBox.Show($"No se encontró la guía '{guia}' en Python (:8082).", "Guía No Encontrada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string getJson = await getResp.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(getJson);
                string estadoActual = GetJsonString(doc.RootElement, "estado", "Estado").ToUpper();

                string nuevoEstado;
                string ubicacion;
                string comentario;

                switch (estadoActual)
                {
                    case "PREPARACION":
                        nuevoEstado = "EN_TRANSITO";
                        ubicacion = "Centro de Distribución Bajío - Silao";
                        comentario = "Paquete clasificado y despachado en tractocamión ruta principal.";
                        break;
                    case "EN_TRANSITO":
                        nuevoEstado = "EN_REPARTO";
                        ubicacion = "Unidad Móvil de Entrega Urbana #42";
                        comentario = "Paquete cargado en vehículo de última milla. En ruta hacia destino.";
                        break;
                    case "EN_REPARTO":
                        nuevoEstado = "ENTREGADO";
                        ubicacion = "Domicilio del Destinatario";
                        comentario = "Paquete entregado al cliente satisfecho. Firma y sello digital registrados.";
                        break;
                    case "ENTREGADO":
                        MessageBox.Show($"El paquete '{guia}' ya se encuentra en su estado final 'ENTREGADO'.", "Entrega Completada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    default:
                        nuevoEstado = "EN_TRANSITO";
                        ubicacion = "Hub Logístico Regional";
                        comentario = $"Estado actualizado desde {estadoActual}.";
                        break;
                }

                string putUrl = GetCleanHost(8082, $"/api/v1/envios/{guia}/estado");
                var payloadObj = new
                {
                    nuevo_estado = nuevoEstado,
                    ubicacion_actual = ubicacion,
                    comentario = comentario
                };
                string jsonBody = JsonSerializer.Serialize(payloadObj);

                using var putReq = new HttpRequestMessage(HttpMethod.Put, putUrl);
                putReq.Headers.Add("Authorization", AUTH_TOKEN);
                putReq.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var putResp = await _httpClient.SendAsync(putReq);
                sw.Stop();
                string putRespJson = await putResp.Content.ReadAsStringAsync();

                LogNetwork("REST (JSON)", "PUT", putUrl, jsonBody, putRespJson, (int)putResp.StatusCode, sw.ElapsedMilliseconds);

                if (putResp.IsSuccessStatusCode)
                {
                    await RastrearGuiaRestAsync();
                }
                else
                {
                    MessageBox.Show($"Error HTTP {(int)putResp.StatusCode} al actualizar estado: {putRespJson}", "Error REST", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar estado del envío en Python (:8082): " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnAvanzarEstado.Enabled = true;
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

        private static string GetJsonString(JsonElement element, params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                if (element.TryGetProperty(name, out var prop))
                {
                    return prop.GetString() ?? "";
                }
            }
            return "";
        }

        private static decimal GetJsonDecimal(JsonElement element, params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                if (element.TryGetProperty(name, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.Number && prop.TryGetDecimal(out var val))
                        return val;
                    if (decimal.TryParse(prop.GetString(), out var parsed))
                        return parsed;
                }
            }
            return 0m;
        }

        private Button CreateNavButton(string text, bool isActive)
        {
            Button btn = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 34,
                Padding = new Padding(14, 4, 14, 4),
                Margin = new Padding(0, 0, 10, 0),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BackColor = isActive ? Color.FromArgb(37, 99, 235) : Color.FromArgb(30, 41, 59),
                ForeColor = isActive ? Color.White : Color.FromArgb(148, 163, 184)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void SetActiveTab(int index, TabControl tabControl, Button[] navButtons)
        {
            tabControl.SelectedIndex = index;
            for (int i = 0; i < navButtons.Length; i++)
            {
                bool active = (i == index);
                navButtons[i].BackColor = active ? Color.FromArgb(37, 99, 235) : Color.FromArgb(30, 41, 59);
                navButtons[i].ForeColor = active ? Color.White : Color.FromArgb(148, 163, 184);
            }
        }
    }
}
