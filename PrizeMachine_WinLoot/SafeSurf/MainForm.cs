using Microsoft.Win32;
using RefreshUtilities;
using SCTVObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SCTV
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1301:AvoidDuplicateAccelerators")]
    public partial class MainForm : Form
    {
        SettingsHelper helper = SettingsHelper.Current;
        private bool loggedIn = false;
        public static string[] blockedTerms;
        public static string[] foundBlockedTerms;
        public static string[] foundBlockedSites;
        public static string blockedTermsPath = "config\\BlockedTerms.txt";
        public static string foundBlockedTermsPath = "config\\FoundBlockedTerms.txt";
        public static string[] blockedSites;
        public static string blockedSitesPath = "config\\BlockedSites.txt";
        public static string foundBlockedSitesPath = "config\\foundBlockedSites.txt";
        public static string loginInfoPath = "config\\LoginInfo.txt";
        public static string statusLogPath = ConfigurationManager.AppSettings["StatusLogPath"]; //"config\\Status_"+ DateTime.Now.ToLongDateString() +"_"+ DateTime.Now.ToLongTimeString() +".txt";
        public static string finishedLogPath = ConfigurationManager.AppSettings["finishedLogPath"];
        public bool adminLock = false;//locks down browser until unlocked by a parent
        public int loggedInTime = 0;
        public bool checkForms = true;
        public bool MonitorActivity = false; //determines whether safesurf monitors page contents, forms, sites, etc...
        int loginMaxTime = 20;//20 minutes
        TabCtlEx tabControlEx = new TabCtlEx();

        bool showVolumeControl = false;
        bool showAddressBar = true;

        private DateTime startTime;
        private string userName;
        Timer keepRunning_tour_Timer = new Timer();
        //Timer secondsTimer = new Timer();
        Timer documentLoaded_tour_Timer = new Timer();
        Timer documentLoaded_tourList_Timer = new Timer();
        //ExtendedWebBrowser hideMeBrowser;
        ExtendedWebBrowser bitVideoBrowser;
        int keepRunningTimerTicks = 0;
        //string goToUrlString = "";
        string startTourUrlString = "";
        public string documentStringLoaded = "";
        int currentPageNumber = 0;
        int nextPageNumber = 0;
        bool tourIsRunning = false;
        ArrayList tourList = new ArrayList();
        int currentTourIndex = 1;
        string currentPageURL = "";
        string previousPageURL = "";
        RefreshUtilities.RefreshUtilities refreshUtilities;
        string documentString = "";
        bool enteredTheContest = false;
        bool foundPrize = false;
        bool foundQuickPick = false;
        bool foundNewContest = false;
        bool foundSubmit = false;
        int numberOfPrizesEntered = 0;
        int numberOfCashtravaganzaEntered = 0;
        int numberOfUnclaimedEntered = 0;
        int refreshCount = 0;
        List<string> users = new List<string>();
        string userLoggingOut = "";
        bool foundOffsiteURL = false;
        bool foundSkipAndContinue = false;
        bool logBackIn = false;
        bool loggingIn = false;
        string currentUser = "";

        public bool LoggedIn
        {
            set
            {
                loggedIn = value;

                if (loggedIn)
                {
                    UpdateLoginToolStripMenuItem.Visible = true;
                    parentalControlsToolStripMenuItem.Visible = true;
                    loginToolStripMenuItem.Visible = false;
                    logoutToolStripMenuItem.Visible = true;
                    logoutToolStripButton.Visible = true;
                    LoginToolStripButton.Visible = false;
                    adminToolStripButton.Visible = true;

                    loginTimer.Enabled = true;
                    loginTimer.Start();
                }
                else
                {
                    UpdateLoginToolStripMenuItem.Visible = false;
                    parentalControlsToolStripMenuItem.Visible = false;
                    loginToolStripMenuItem.Visible = true;
                    logoutToolStripMenuItem.Visible = false;
                    logoutToolStripButton.Visible = false;
                    LoginToolStripButton.Visible = true;
                    adminToolStripButton.Visible = false;
                    tcAdmin.Visible = false;

                    loginTimer.Enabled = false;
                    loginTimer.Stop();
                }
            }

            get
            {
                return loggedIn;
            }
        }

        public Uri URL
        {
            set { _windowManager.ActiveBrowser.Url = value; }
            get { return _windowManager.ActiveBrowser.Url; }
        }

        public bool ShowMenuStrip
        {
            set { this.menuStrip.Visible = value; }
        }

        public FormBorderStyle FormBorder
        {
            set { this.FormBorderStyle = value; }
        }

        public bool ShowLoginButton
        {
            set { LoginToolStripButton.Visible = value; }
        }

        public bool ShowJustinRecordButton
        {
            set { JustinRecordtoolStripButton.Visible = value; }
        }

        public bool ShowVolumeControl
        {
            set
            {
                showVolumeControl = value;
                //volumeControl.Visible = value; 
            }

            get { return showVolumeControl; }
        }

        public bool ShowAddressBar
        {
            set { showAddressBar = value; }

            get { return showAddressBar; }
        }

        public string SetDocumentString
        {
            set
            {
                //documentLoaded_tour(value);
            }
        }

        public string SetTourListDocumentString
        {
            set
            {
                //documentLoaded_tourList(value);
            }
        }

        public MainForm()
        {
            InitializeComponent();

            try
            {
                if(!statusLogPath.Contains("."))
                    statusLogPath += "Status_"+ DateTime.Now.ToShortDateString().Replace("/","") +"_"+ DateTime.Now.ToShortTimeString().Replace(":","") +".txt";

                statusLogPath = statusLogPath.Replace(" ", "");

                useLatestIE();

                tabControlEx.Name = "tabControlEx";
                tabControlEx.SelectedIndex = 0;
                tabControlEx.Visible = false;
                tabControlEx.OnClose += new TabCtlEx.OnHeaderCloseDelegate(tabEx_OnClose);
                tabControlEx.VisibleChanged += new System.EventHandler(this.tabControlEx_VisibleChanged);

                this.panel1.Controls.Add(tabControlEx);
                tabControlEx.Dock = DockStyle.Fill;

                _windowManager = new WindowManager(tabControlEx);
                _windowManager.CommandStateChanged += new EventHandler<CommandStateEventArgs>(_windowManager_CommandStateChanged);
                _windowManager.StatusTextChanged += new EventHandler<TextChangedEventArgs>(_windowManager_StatusTextChanged);
                //_windowManager.DocumentCompleted += tour_DocumentCompleted;
                //_windowManager.ActiveBrowser.Navigating += ActiveBrowser_Navigating;
                //_windowManager.ActiveBrowser.ScriptErrorsSuppressed = true;
                _windowManager.ShowAddressBar = showAddressBar;

                showAddressBarToolStripMenuItem.Checked = showAddressBar;

                startTime = DateTime.Now;
                userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                initFormsConfigs();

                users.Add("lickey10@gmail.com|soccer");
                users.Add("lickeykids@gmail.com|soccer");

                //getDefaultBrowser();

            }
            catch (Exception ex)
            {
                //Tools.WriteToFile(ex);
                //Application.Restart();
            }
        }

        // Starting the app here...
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Open a new browser window

                //hideMeBrowser = _windowManager.New(false);
                //hideMeBrowser.Url = new Uri("https://us.hideproxy.me/index.php");



                bitVideoBrowser = this._windowManager.New();
                bitVideoBrowser.DocumentCompleted += MainBrowser_DocumentCompleted;
                bitVideoBrowser.Url = new Uri("http://www.winloot.com/Sweepstake");

                //bitVideoBrowser.StartNewWindow += BitVideoBrowser_StartNewWindow;

                try
                {
                    bitVideoBrowser.ObjectForScripting = new GetTourListDocumentString();
                }
                catch (Exception ex)
                {
                    throw;
                }

                refreshUtilities = new RefreshUtilities.RefreshUtilities();
                refreshUtilities.ClickComplete += RefreshUtilities_ClickComplete;
                refreshUtilities.CallMethodComplete += RefreshUtilities_CallMethodComplete;
                refreshUtilities.GoToUrlComplete += RefreshUtilities_GoToUrlComplete;
            }
            catch (Exception ex)
            {
                Tools.WriteToFile(ex);
                //Application.Restart();
            }
        }

        private void RefreshUtilities_GoToUrlComplete(object sender, EventArgs e)
        {
            if (sender != null && sender is RefreshUtilities.TimerInfo && ((RefreshUtilities.TimerInfo)sender).Browser is ExtendedWebBrowser)
            {
                ExtendedWebBrowser tempBrowser = (ExtendedWebBrowser)((RefreshUtilities.TimerInfo)sender).Browser;

                if (tempBrowser.IsBusy)
                    tempBrowser.Stop();

                tempBrowser.Url = new Uri(((RefreshUtilities.TimerInfo)sender).UrlToGoTo);
            }
        }

        private void RefreshUtilities_CallMethodComplete(object sender, EventArgs e)
        {
            if (((TimerInfo)sender).MethodToCall == "javascript:useFavorites()")
            {
                findSubmit(bitVideoBrowser.Document);
            }
        }

        private void RefreshUtilities_ClickComplete(object sender, EventArgs e)
        {
            //if the sender is the quick pick button then find and click the submit button
            try
            {
                if (!bitVideoBrowser.Document.Url.ToString().Contains("www.winloot.com/Sweepstake/ShowSubmit/") && (((HtmlElement)sender).GetAttribute("value") == "QUICK PICKS" || ((HtmlElement)sender).GetAttribute("value") == "USE QUICK PICKS" || ((HtmlElement)sender).GetAttribute("src").Contains("/images/bonusgame/button_bonusgame_autopick_on.png")
                                || ((HtmlElement)sender).OuterHtml.Contains("img-responsive prev-on") || ((HtmlElement)sender).GetAttribute("href") == "javascript:useFavorites()"
                                || ((HtmlElement)sender).OuterHtml.Contains("userFavorites()") || ((HtmlElement)sender).GetAttribute("href") == "javascript:quickPicks()"))//this is the quick pick button - now click the submit button
                {
                    findSubmit(bitVideoBrowser.Document);
                        //refreshUtilities.CallMethod("javascript:useFavorites()", true, lblRefreshTimer);
                }

                if (((HtmlElement)sender).GetAttribute("src").Contains("/images/bonusgame/button_bonusgame_autopick_on.png"))
                {
                    if (!findSkipAndContinue())
                    {
                        foundQuickPick = false;
                        refreshUtilities.GoToURL("http://www.winloot.com/Sweepstake", true, lblRefreshTimer, bitVideoBrowser);
                    }
                }
                else if (!loggingIn)
                {
                    foundQuickPick = false;
                    refreshUtilities.GoToURL("http://www.winloot.com/", 19, lblRefreshTimer, bitVideoBrowser);
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }

        private void BitVideoBrowser_StartNewWindow(object sender, BrowserExtendedNavigatingEventArgs e)
        {
            //if (tourBrowser == null || !tourBrowser.Created)
            //{
            //    tourBrowser = this._windowManager.New();
            //    //tourBrowser.DocumentCompleted += tour_DocumentCompleted;
            //    tourBrowser.Downloading += TourBrowser_Downloading;
            //    tourBrowser.DownloadComplete += TourBrowser_DownloadComplete;
            //    tourBrowser.Navigating += TourBrowser_Navigating;
            //    tourBrowser.Url = e.Url;
            //    tourIsRunning = true;

            //    try
            //    {
            //        tourBrowser.ObjectForScripting = new GetTourDocumentString();
            //    }
            //    catch (Exception ex)
            //    {
            //        throw;
            //    }
            //}
        }

        //private void TourBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        //{
        //    lblDownloading.BackColor = Color.Green;
        //}

        //private void TourBrowser_DownloadComplete(object sender, EventArgs e)
        //{
        //    lblStreaming.BackColor = Color.Red;
        //}

        //private void TourBrowser_Downloading(object sender, EventArgs e)
        //{
        //    lblStreaming.BackColor = Color.Green;
        //}

        private void Window_Error(object sender, HtmlElementErrorEventArgs e)
        {
            //Application.Restart();
        }

        private void ActiveBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            //documentString = "";
        }

        private void MainBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                documentString = _windowManager.ActiveBrowser.DocumentText;

                if (currentUser.Length == 0)
                    currentUser = getCurrentUser();

                if (bitVideoBrowser.Url != null)
                {
                    if (!bitVideoBrowser.Url.Host.ToLower().Contains("www.winloot.com") && !foundOffsiteURL)
                    {
                        foundQuickPick = false;
                        foundOffsiteURL = true;

                        //if (bitVideoBrowser.Url.Host.ToLower() == "offers.winloot.com" || bitVideoBrowser.Url.Host.ToLower() == "entries.winloot.com")
                        //    findSkipAndContinue();
                        //else
                        refreshUtilities.GoToURL("http://www.winloot.com/Sweepstake", true, lblRefreshTimer, bitVideoBrowser);
                    }
                    else if (bitVideoBrowser.Url.Host.ToLower().Contains("www.winloot.com") && !documentString.ToLower().Contains("logout"))//need to login
                    {
                        refreshUtilities.Cancel();
                        lblRefreshTimer.Text = "0 seconds";

                        if (!loggingIn || logBackIn)
                        {
                            foreach (string user in users)
                            {
                                if (userLoggingOut != user)
                                {
                                    logIn(user.Split('|')[0], user.Split('|')[1]);

                                    break;
                                }
                            }
                        }

                        //check for play now button
                        if (!loggingIn)
                        {
                            //look for join button
                            string joinButton = findValue(bitVideoBrowser.DocumentText, "<div class=\"ucase JoinButton\" style=\"padding-left:20px;padding-right:20px;", "</div>");

                            if (joinButton.Contains("Play Now!</a>"))
                            {
                                string playNowLink = findValue(joinButton, "<a href=\"", "\">Play Now!</a>");
                                refreshUtilities.GoToURL(bitVideoBrowser.Url.Host + playNowLink, lblRefreshTimer, bitVideoBrowser);
                            }
                        }
                    }
                    else if (bitVideoBrowser.Url.ToString().ToLower().Contains("://www.winloot.com/sweepstake") || bitVideoBrowser.Url.ToString().ToLower().Contains("://www.winloot.com/5k_sweepstakes") || bitVideoBrowser.Url.ToString().ToLower().Contains("://www.winloot.com/bonusgame"))
                    {
                        foundOffsiteURL = false;
                        loggingIn = false;

                        if (bitVideoBrowser.Document.Url.ToString().ToLower().Contains("www.winloot.com/sweepstake/showsubmit/"))
                        {
                            if (!foundSubmit || previousPageURL != bitVideoBrowser.Document.Url.ToString())
                            {
                                refreshUtilities.Cancel();

                                previousPageURL = bitVideoBrowser.Document.Url.ToString();

                                foundSubmit = findSubmit(bitVideoBrowser.Document);
                            }
                        }
                        else if (!foundQuickPick)
                        {
                            if (!findQuickPick(bitVideoBrowser.Document))
                            {
                                //if (!findNextContestLink(bitVideoBrowser.DocumentText) && refreshCount < 1)
                                //{
                                //    refreshCount++;
                                //    refreshUtilities.GoToURL(bitVideoBrowser.Url.ToString(), lblRefreshTimer, bitVideoBrowser);
                                //}
                            }
                        }
                        //else if (!foundSubmit)//we have found quickpick but not the submit button
                        //{
                        //    if (!findSubmit(bitVideoBrowser.Document) && refreshCount < 1)
                        //    {

                        //    }
                        //}
                        //else if (!foundNewContest)// we have found both quick pick and submit now we need to look for the next contest link
                        //{
                        //    if (!findNextContestLink(bitVideoBrowser.DocumentText) && refreshCount < 1)
                        //    {
                        //        refreshCount++;
                        //        refreshUtilities.GoToURL(bitVideoBrowser.Url.ToString(), lblRefreshTimer, bitVideoBrowser);
                        //    }
                        //}
                        //else if(!bitVideoBrowser.Url.ToString().ToLower().Contains("/index/") && !foundNewContest) //look for the link for the next contest
                        //    findNextContestLink(bitVideoBrowser.DocumentText);
                    }
                    else if (bitVideoBrowser.Url.ToString().ToLower().StartsWith("https://www.winloot.com/status/index?"))// if we have a currentUser then we are done and need to switch users
                    {
                        if (currentUser.Length > 0)
                        {
                            btnSwitchUsers_Click(null, null);
                        }
                    }
                    else if (bitVideoBrowser.Url.Host.ToLower().Contains("www.winloot.com") && !bitVideoBrowser.Url.ToString().ToLower().Contains("/index/")) //look for the link for the next contest
                    {
                        if (!foundNewContest)
                        {
                            foundOffsiteURL = false;
                            foundQuickPick = false;

                            findNextContestLink(bitVideoBrowser.DocumentText);
                        }
                    }
                    else
                        refreshUtilities.GoToURL("http://www.winloot.com/Sweepstake", 20, false, lblRefreshTimer, bitVideoBrowser);
                }
            }
            catch (Exception ex)
            {
                throw;
                //Application.Restart();
            }
        }

        private bool findQuickPick(HtmlDocument pageDocument)
        {
            if (!foundQuickPick)
            {
                foundNewContest = false;

                //HtmlElementCollection elc = pageDocument.GetElementsByTagName("div");
                ////<div style="cursor: pointer;" class="gb game-btn3" onclick="userFavorites()"></div>
                ////<div style="cursor: pointer;" class="gb game-btn1 disable hidden" onclick="clearNumbers();"></div>
                //foreach (HtmlElement el in elc)
                //{
                //    if (el.OuterHtml != null && el.OuterHtml.Contains("userFavorites()") && !el.OuterHtml.Contains("disabled") && !el.OuterHtml.Contains("clearNumbers()"))
                //    {
                //        refreshUtilities.ClickElement(el, lblRefreshTimer);
                //        foundQuickPick = true;
                //        refreshCount = 0;

                //        return true;
                //    }
                //}

                HtmlElementCollection elc = pageDocument.GetElementsByTagName("input");
                //<a href="javascript:useFavorites()" class="btn_useFavorites"><img src="/images/bs-responsive/use-faves-on.png" alt="Use Faves" class="img-responsive"></a>
                //<a onclick="userFavorites()"><img src="//static.winloot.com/images/spacer.gif" class="img-responsive"></a>
                foreach (HtmlElement el in elc)
                {
                    //<a aria-label="Use favorites" onclick="userFavorites()"><img src="//static.winloot.com/images/spacer.gif" class="img-responsive"></a>
                    //<input class="btn btn-primary2 btn-block btn-sm btn-usefaves disable text-center " aria-label="Favorites" onclick="userFavorites()" type="button" value="USE FAVORITES">
                    if (el.OuterHtml != null && el.OuterHtml.ToLower().Contains("onclick=\"userfavorites()\"") && !el.OuterHtml.ToLower().Contains(" disable"))
                    {
                        refreshUtilities.ClickElement(el, true, lblRefreshTimer);
                        foundQuickPick = true;
                        refreshCount = 0;

                        return true;
                    }
                }

                //first look for the previous picks button
                elc = pageDocument.GetElementsByTagName("img");
                //<img src="/images/bonusgame/button_bonusgame_previouspicks_on.png" class="img-responsive prev-on ">
                //<img src="/images/bonusgame/button_bonusgame_previouspicks_off.png" class="img-responsive prev-off disabled">
                foreach (HtmlElement el in elc)
                {
                    if (el.OuterHtml != null && el.OuterHtml.Contains("img-responsive prev-on") && !el.OuterHtml.Contains("disabled") && !el.OuterHtml.ToLower().Contains("pull-left"))
                    {
                        refreshUtilities.ClickElement(el, true, lblRefreshTimer);
                        foundQuickPick = true;
                        refreshCount = 0;

                        return true;
                    }
                }

                elc = pageDocument.GetElementsByTagName("input");

                foreach (HtmlElement el in elc)
                {
                    if (el.GetAttribute("value") == "QUICK PICKS" || el.GetAttribute("value") == "USE QUICK PICKS")
                    {
                        refreshUtilities.ClickElement(el, true, lblRefreshTimer);
                        foundQuickPick = true;
                        refreshCount = 0;

                        return true;
                    }
                }

                //<img src="//static.winloot.com/images/spacer.gif" class="img-responsive">
                elc = pageDocument.GetElementsByTagName("a");

                foreach (HtmlElement el in elc)
                {
                    if (el.GetAttribute("href") == "javascript:quickPicks()" || (el.OuterHtml != null && el.OuterHtml.ToLower().Contains("onclick=\"userfavorites()\"") && !el.OuterHtml.ToLower().Contains(" disable")))
                    {
                        refreshUtilities.ClickElement(el, true, lblRefreshTimer);
                        foundQuickPick = true;
                        refreshCount = 0;

                        return true;
                    }
                }

                elc = pageDocument.GetElementsByTagName("img");

                foreach (HtmlElement el in elc)
                {
                    if (el.GetAttribute("src").Contains("/images/bonusgame/button_bonusgame_autopick_on.png"))
                    {
                        refreshUtilities.ClickElement(el, true, lblRefreshTimer);
                        foundQuickPick = true;
                        refreshCount = 0;

                        return true;
                    }
                }


                //<img src="//static.winloot.com/images/spacer.gif" class="img-responsive">
                elc = pageDocument.GetElementsByTagName("img");

                foreach (HtmlElement el in elc)
                {
                    if (el.GetAttribute("class") == "img-responsive")
                    {
                        refreshUtilities.ClickElement(el, true, lblRefreshTimer);
                        foundQuickPick = true;
                        refreshCount = 0;

                        return true;
                    }
                }

                //refreshUtilities.GoToURL("javascript:quickPicks()", lblRefreshTimer, bitVideoBrowser);
                //foundQuickPick = true;
                //refreshCount = 0;

                //return true;
            }

            return false;
        }

        private bool findSubmit(HtmlDocument pageDocument)
        {
            HtmlElementCollection elc = pageDocument.GetElementsByTagName("div");
            bool submitButtonIsNext = false;

            foreach (HtmlElement el in elc)
            {
                //if(el.OuterHtml != null && el.OuterHtml.Contains("class=\"hidden-xs partnersite-submit subSweep\"") && el.InnerText == null && el.InnerHtml == null)//this is the submit button - click it
                if (el.OuterHtml != null && el.OuterHtml.Contains("subSweep") && el.InnerText == null && el.InnerHtml == null)//this is the submit button - click it
                {
                    refreshUtilities.ClickElement(el, lblRefreshTimer);
                    foundSubmit = true;
                    refreshCount = 0;
                    submitButtonIsNext = false;
                    numberOfPrizesEntered++;
                    txtPrizeCount.Text = numberOfPrizesEntered.ToString();

                    log(statusLogPath, currentUser +" - Entered another contest " + numberOfPrizesEntered.ToString());

                    return true;
                }

                if (el.InnerText == "CLICK THE BUTTON BELOW TO SUBMIT YOUR NUMBERS")
                    submitButtonIsNext = true;
            }

            elc = pageDocument.GetElementsByTagName("img");

            foreach (HtmlElement el in elc)
            {
                if (el.GetAttribute("src").Contains("/images/bonusgame/button_bonusgame_go_on.png"))
                {
                    refreshUtilities.ClickElement(el, lblRefreshTimer);
                    foundQuickPick = false;
                    refreshCount = 0;
                    numberOfPrizesEntered++;
                    txtPrizeCount.Text = numberOfPrizesEntered.ToString();

                    log(statusLogPath, currentUser + " - Entered another contest " + numberOfPrizesEntered.ToString());

                    return true;
                }
            }

            return false;
        }

        private bool findNextContestLink(string pageContent)
        {
            foundQuickPick = false;
            foundSubmit = false;
            //string nextContestLink = findValue(pageContent, "window.location='/DailyDraw/Index", "'");
            string nextContestLink = findValue(pageContent, "window.location='/", "'");

            if (nextContestLink.ToLower().Contains("/blog/"))
                foundQuickPick = true;

            if (nextContestLink.Trim().Length == 0)
            {
                nextContestLink = findValue(pageContent, "/Sweepstake/Index", "\"");

                if (nextContestLink.Trim().Length == 0)
                    nextContestLink = "Sweepstake/Index" + nextContestLink;
            }

            if (nextContestLink.Trim().Length > 0)
            {
                nextContestLink = bitVideoBrowser.Document.Url.Scheme + "://www.winloot.com/" + nextContestLink;

                foundNewContest = true;

                refreshUtilities.GoToURL(nextContestLink, true, lblRefreshTimer, bitVideoBrowser);

                return true;
            }
            else //we are done - switch users
                btnSwitchUsers.PerformClick();

            return false;
        }

        private bool findSkipAndContinue()
        {
            bool foundSkip = false;

            //HtmlElementCollection elc = bitVideoBrowser.Document.GetElementsByTagName("a");

            ////find user
            //foreach (HtmlElement el in elc)
            //{
            //    //<a class="js-skip-sweep" data-slug="250_Gift_Card_Sweepstakes" href="https://www.winloot.com">SKIP &amp; CONTINUE TO THE NEXT SWEEPS</a>
            //    if (el.InnerText != null && (el.InnerText.ToUpper().Contains("CONTINUE TO THE NEXT SWEEPS") || el.InnerText.ToLower().Contains("skip this poll")))//this is the click and continue link
            //    {
            //        refreshUtilities.ClickElement(el, 18, lblRefreshTimer);

            //        foundSkip = true;

            //        break;
            //    }
            //}


            refreshUtilities.GoToURL("https://www.winloot.com", true, lblRefreshTimer, bitVideoBrowser);

            foundSkip = true;

            return foundSkip;
        }

        private string getCurrentUser()
        {
            string tempUser = "";
            string tempPassword = "";

            foreach (string user in users)
            {
                tempUser = user.Split('|')[0];
                tempPassword = user.Split('|')[1];

                if (bitVideoBrowser.DocumentText.ToLower().Contains(tempUser.ToLower()))
                {
                    return tempUser;
                }
            }

            return "";
        }

        private void logIn(string username, string password)
        {
            userLoggingOut = "";
            logBackIn = false;
            bool foundEmail = false;
            bool foundPassword = false;
            HtmlElement txtPassword = null;

            log(statusLogPath, "Logging In "+ username);

            HtmlElementCollection elc = bitVideoBrowser.Document.GetElementsByTagName("input");

            //find user
            foreach (HtmlElement el in elc)
            {
                //<input placeholder="Email" type="email" class="form-control form-group-sm" name="email" tabindex="1">
                if (el.OuterHtml.ToLower().Contains("type=\"email\""))//this is the email field
                {
                    //el.Focus();
                    //el.InnerText = username;
                    el.SetAttribute("value", username);

                    foundEmail = true;
                }

                //<input placeholder="Password" type="password" class="form-control form-group-sm" name="password" tabindex="2">
                if (el.OuterHtml.ToLower().Contains("tabindex=\"2\""))//this is the password field
                {
                    //el.SetAttribute("text", password);
                    el.SetAttribute("value", password);
                    txtPassword = el;
                    foundPassword = true;

                    break;
                }

                if (!foundEmail && el.OuterHtml.ToLower().Contains("id=\"new_landing_email\"")) //check for the new email field
                {
                    //<input id="new_landing_email" class="form-control input-lg xverify_email" value="" type="text" style="color:black" placeholder="Type Your Email Here...">
                    el.SetAttribute("value", username);

                    foundEmail = true;

                    break;
                }
            }

            if (foundEmail && foundPassword)
            {
                elc = bitVideoBrowser.Document.Forms;

                //elc = bitVideoBrowser.Document.GetElementsByTagName("form");

                foreach (HtmlElement el in elc)
                {
                    //<form id="simpleLogin-form" action="/Home/Login" method="POST" class="hidden-xs">
                    if (el.OuterHtml.Contains("id=\"simpleLogin-form\"") && el.OuterHtml.Contains("class=\"hidden-xs\""))
                    {
                        //submit the form
                        //el.InvokeMember("submit");
                        //loggingIn = true;
                        //break;

                        HtmlElementCollection elc2 = el.GetElementsByTagName("input");

                        foreach (HtmlElement el2 in elc2)
                        {
                            //find login

                            //<input type="submit" class="WLButton ucase btn btn-success form-control loginbtn" value="Login" tabindex="3">
                            if (el2.OuterHtml.ToLower().Contains("type=\"submit\"") && el2.OuterHtml.ToLower().Contains("value=\"login\""))//this is the login button
                            {
                                loggingIn = true;
                                refreshUtilities.ClickElement(el2, 3, true, lblRefreshTimer);

                                currentUser = username + "|" + password;

                                break;
                            }
                        }
                    }
                }
            }
            else if(foundEmail)
            {
                elc = bitVideoBrowser.Document.GetElementsByTagName("button");

                foreach (HtmlElement el in elc)
                {
                    //<button type="button" value="" class="text-center" id="regSubmit">YES, I WANT TO WIN</button>
                    if (el.OuterHtml.Contains("id=\"regSubmit\""))
                    {
                        //click the button

                        loggingIn = true;
                        refreshUtilities.ClickElement(el, 3, true, lblRefreshTimer);

                        currentUser = username + "|" + password;

                        break;
                    }
                }
            }
        }

        private void logout()
        {
            log(statusLogPath, "Logging Out "+ currentUser);

            currentUser = "";
            refreshUtilities.GoToURL("https://www.winloot.com/Home/Logout", 1, lblRefreshTimer, bitVideoBrowser);
        }

        private void initFormsConfigs()
        {
            checkForms = helper.CheckForms;
        }

        private void useLatestIE()
        {
            try
            {
                string AppName = Application.ProductName;// My.Application.Info.AssemblyName
                int VersionCode = 0;
                string Version = "";
                object ieVersion = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Internet Explorer").GetValue("svcUpdateVersion");

                if (ieVersion == null)
                    ieVersion = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Internet Explorer").GetValue("Version");

                if (ieVersion != null)
                {
                    Version = ieVersion.ToString().Substring(0, ieVersion.ToString().IndexOf("."));
                    switch (Version)
                    {
                        case "7":
                            VersionCode = 7000;
                            break;
                        case "8":
                            VersionCode = 8888;
                            break;
                        case "9":
                            VersionCode = 9999;
                            break;
                        case "10":
                            VersionCode = 10001;
                            break;
                        default:
                            if (int.Parse(Version) >= 11)
                                VersionCode = 11001;
                            else
                                Tools.WriteToFile(Tools.errorFile, "useLatestIE error: IE Version not supported");
                            break;
                    }
                }
                else
                {
                    Tools.WriteToFile(Tools.errorFile, "useLatestIE error: Registry error");
                }

                //'Check if the right emulation is set
                //'if not, Set Emulation to highest level possible on the user machine
                string Root = "HKEY_CURRENT_USER\\";
                string Key = "Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION";

                object CurrentSetting = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(Key).GetValue(AppName + ".exe");

                if (CurrentSetting == null || int.Parse(CurrentSetting.ToString()) != VersionCode)
                {
                    Microsoft.Win32.Registry.SetValue(Root + Key, AppName + ".exe", VersionCode);
                    Microsoft.Win32.Registry.SetValue(Root + Key, AppName + ".vshost.exe", VersionCode);
                }
            }
            catch (Exception ex)
            {
                Tools.WriteToFile(Tools.errorFile, "useLatestIE error: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        // Update the status text
        void _windowManager_StatusTextChanged(object sender, TextChangedEventArgs e)
        {
            this.toolStripStatusLabel.Text = e.Text;
        }

        // Enable / disable buttons
        void _windowManager_CommandStateChanged(object sender, CommandStateEventArgs e)
        {
            this.forwardToolStripButton.Enabled = ((e.BrowserCommands & BrowserCommands.Forward) == BrowserCommands.Forward);
            this.backToolStripButton.Enabled = ((e.BrowserCommands & BrowserCommands.Back) == BrowserCommands.Back);
            this.printPreviewToolStripButton.Enabled = ((e.BrowserCommands & BrowserCommands.PrintPreview) == BrowserCommands.PrintPreview);
            this.printPreviewToolStripMenuItem.Enabled = ((e.BrowserCommands & BrowserCommands.PrintPreview) == BrowserCommands.PrintPreview);
            this.printToolStripButton.Enabled = ((e.BrowserCommands & BrowserCommands.Print) == BrowserCommands.Print);
            this.printToolStripMenuItem.Enabled = ((e.BrowserCommands & BrowserCommands.Print) == BrowserCommands.Print);
            this.homeToolStripButton.Enabled = ((e.BrowserCommands & BrowserCommands.Home) == BrowserCommands.Home);
            this.searchToolStripButton.Enabled = ((e.BrowserCommands & BrowserCommands.Search) == BrowserCommands.Search);
            this.refreshToolStripButton.Enabled = ((e.BrowserCommands & BrowserCommands.Reload) == BrowserCommands.Reload);
            this.stopToolStripButton.Enabled = ((e.BrowserCommands & BrowserCommands.Stop) == BrowserCommands.Stop);
        }

        #region Tools menu
        // Executed when the user clicks on Tools -> Options
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OptionsForm of = new OptionsForm())
            {
                of.ShowDialog(this);
            }
        }

        // Tools -> Show script errors
        private void scriptErrorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptErrorManager.Instance.ShowWindow();
        }

        //login to be able to access/modify blockedTerms file
        private void loginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Login login = new Login())
            {
                login.ShowDialog(this);
                if (login.DialogResult == DialogResult.Yes)
                {
                    LoggedIn = true;
                    adminLock = false;
                }
                else if (login.DialogResult == DialogResult.None)
                    adminLock = true;
                else
                    LoggedIn = false;
            }
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoggedIn = false;
        }

        private void UpdateLoginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Login login = new Login())
            {
                login.Update = true;
                login.ShowDialog(this);
            }
        }

        private void modifyBlockedTermsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //display terms
            tcAdmin.Visible = true;
            tcAdmin.BringToFront();

            tcAdmin.SelectedTab = tcAdmin.TabPages["tpChangeLoginInfo"];
        }

        private void modifyBlockedSitesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tcAdmin.Visible = true;
            tcAdmin.BringToFront();
            tcAdmin.SelectedTab = tcAdmin.TabPages["tpBlockedSites"];
        }

        private void foundBlockedTermsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tcAdmin.Visible = true;
            tcAdmin.BringToFront();
            tcAdmin.SelectedTab = tcAdmin.TabPages["tpFoundBlockedTerms"];
        }

        private void foundBlockedSitesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tcAdmin.Visible = true;
            tcAdmin.BringToFront();
            tcAdmin.SelectedTab = tcAdmin.TabPages["tpFoundBlockedSites"];
        }
        #endregion

        #region File Menu

        // File -> Print
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Print();
        }

        // File -> Print Preview
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintPreview();
        }

        // File -> Exit
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // File -> Open URL
        private void openUrlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenUrlForm ouf = new OpenUrlForm())
            {
                if (ouf.ShowDialog() == DialogResult.OK)
                {
                    ExtendedWebBrowser brw = _windowManager.New(false);
                    brw.Navigate(ouf.Url);
                }
            }
        }

        // File -> Open File
        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = Properties.Resources.OpenFileDialogFilter;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Uri url = new Uri(ofd.FileName);
                    WindowManager.Open(url);
                }
            }
        }
        #endregion

        #region Help Menu

        // Executed when the user clicks on Help -> About
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About();
        }

        /// <summary>
        /// Shows the AboutForm
        /// </summary>
        private void About()
        {
            using (AboutForm af = new AboutForm())
            {
                af.ShowDialog(this);
            }
        }

        #endregion

        /// <summary>
        /// The WindowManager class
        /// </summary>
        public WindowManager _windowManager;

        // This is handy when all the tabs are closed.
        private void tabControlEx_VisibleChanged(object sender, EventArgs e)
        {
            if (tabControlEx.Visible)
            {
                this.panel1.BackColor = SystemColors.Control;
            }
            else
                this.panel1.BackColor = SystemColors.AppWorkspace;
        }

        #region Printing & Print Preview
        private void Print()
        {
            ExtendedWebBrowser brw = _windowManager.ActiveBrowser;
            if (brw != null)
                brw.ShowPrintDialog();
        }

        private void PrintPreview()
        {
            ExtendedWebBrowser brw = _windowManager.ActiveBrowser;
            if (brw != null)
                brw.ShowPrintPreviewDialog();
        }
        #endregion

        #region Toolstrip buttons
        //private void openWindowToolStripButton_Click(object sender, EventArgs e)
        //{
        //    ExtendedWebBrowser newBrowser = this._windowManager.New();

        //    newBrowser.ObjectForScripting = new GetTourDocumentString();
        //}

        private void closeToolStripButton_Click(object sender, EventArgs e)
        {
            //closes browser window
            //this._windowManager.Close();

            //closes admin tabPages
            tcAdmin.Visible = false;
        }

        private void tabEx_OnClose(object sender, CloseEventArgs e)
        {
            //this.userControl11.Controls.Remove(this.userControl11.TabPages[e.TabIndex]);

            //closes browser window
            this._windowManager.Close();
        }

        private void printToolStripButton_Click(object sender, EventArgs e)
        {
            Print();
        }

        private void printPreviewToolStripButton_Click(object sender, EventArgs e)
        {
            PrintPreview();
        }

        private void backToolStripButton_Click(object sender, EventArgs e)
        {
            if (_windowManager.ActiveBrowser != null && _windowManager.ActiveBrowser.CanGoBack)
                _windowManager.ActiveBrowser.GoBack();
        }

        private void forwardToolStripButton_Click(object sender, EventArgs e)
        {
            if (_windowManager.ActiveBrowser != null && _windowManager.ActiveBrowser.CanGoForward)
                _windowManager.ActiveBrowser.GoForward();
        }

        private void stopToolStripButton_Click(object sender, EventArgs e)
        {
            if (_windowManager.ActiveBrowser != null)
            {
                _windowManager.ActiveBrowser.Stop();
            }
            stopToolStripButton.Enabled = false;
        }

        private void refreshToolStripButton_Click(object sender, EventArgs e)
        {
            if (_windowManager.ActiveBrowser != null)
            {
                _windowManager.ActiveBrowser.Refresh(WebBrowserRefreshOption.Normal);
            }
        }

        private void homeToolStripButton_Click(object sender, EventArgs e)
        {
            if (_windowManager.ActiveBrowser != null)
                _windowManager.ActiveBrowser.GoHome();
        }

        private void searchToolStripButton_Click(object sender, EventArgs e)
        {
            if (_windowManager.ActiveBrowser != null)
                _windowManager.ActiveBrowser.GoSearch();
        }

        #endregion

        public WindowManager WindowManager
        {
            get { return _windowManager; }
        }

        /// <summary>
        /// load blocked terms from file
        /// </summary>
        /// <param name="path"></param>
        public void loadBlockedTerms(string path)
        {
            blockedTerms = File.ReadAllLines(path);

            if (!validateBlockedTerms())
            {
                //decrypt terms
                blockedTerms = Encryption.Decrypt(blockedTerms);
            }

            if (!validateBlockedTerms())
            {
                //log that terms have been tampered with
                log(blockedTermsPath, "Blocked Terms file has been tampered with.  Reinstall SafeSurf");
                //block all pages
                adminLock = true;
            }

            dgBlockedTerms.Dock = DockStyle.Fill;
            dgBlockedTerms.Anchor = AnchorStyles.Right;
            dgBlockedTerms.Anchor = AnchorStyles.Bottom;
            dgBlockedTerms.Anchor = AnchorStyles.Left;
            dgBlockedTerms.Anchor = AnchorStyles.Top;
            dgBlockedTerms.Columns.Add("Terms", "Terms");
            dgBlockedTerms.Refresh();

            foreach (string term in blockedTerms)
            {
                dgBlockedTerms.Rows.Add(new string[] { term });
            }
        }

        private void loadBlockedSites(string path)
        {
            blockedSites = File.ReadAllLines(path);

            if (!validateBlockedSites())
            {
                //decrypt terms
                blockedSites = Encryption.Decrypt(blockedSites);
            }

            if (!validateBlockedSites())
            {
                //log that terms have been tampered with
                log(blockedSitesPath, "Blocked Sites file has been tampered with.  Reinstall SafeSurf");
                //block all pages
                adminLock = true;
            }

            dgBlockedSites.Dock = DockStyle.Fill;
            dgBlockedSites.Anchor = AnchorStyles.Right;
            dgBlockedSites.Anchor = AnchorStyles.Bottom;
            dgBlockedSites.Anchor = AnchorStyles.Left;
            dgBlockedSites.Anchor = AnchorStyles.Top;
            dgBlockedSites.Columns.Add("Sites", "Sites");

            foreach (string site in blockedSites)
            {
                dgBlockedSites.Rows.Add(new string[] { site });
            }
        }

        public void loadFoundBlockedTerms(string path)
        {
            string fBlockedTerms = "";

            if (File.Exists(path))
                foundBlockedTerms = File.ReadAllLines(path);

            if (foundBlockedTerms != null && foundBlockedTerms.Length > 0)
            {
                //if (!validateFoundBlockedTerms())
                //{
                //decrypt terms
                foundBlockedTerms = Encryption.Decrypt(foundBlockedTerms);
                //}

                if (!validateBlockedTerms())
                {
                    //log that terms have been tampered with
                    log(foundBlockedTermsPath, "Found Blocked Terms file has been tampered with.");
                    //block all pages
                    adminLock = true;
                }

                lbFoundBlockedTerms.DataSource = foundBlockedTerms;
            }
        }

        public void loadFoundBlockedSites(string path)
        {
            if (File.Exists(path))
                foundBlockedSites = File.ReadAllLines(path);

            if (foundBlockedSites != null && foundBlockedSites.Length > 0)
            {

                //if (!validateBlockedTerms())
                //{
                //decrypt terms
                foundBlockedSites = Encryption.Decrypt(foundBlockedSites);
                //}

                //if (!validateBlockedTerms())
                //{
                //    //log that terms have been tampered with
                //    log(blockedTermsPath, "Blocked Terms file has been tampered with.  Reinstall SafeSurf");
                //    //block all pages
                //    adminLock = true;
                //}

                lbFoundBlockedSites.DataSource = foundBlockedSites;
            }
        }

        private bool validateBlockedTerms()
        {
            bool isValid = false;

            foreach (string term in blockedTerms)
            {
                if (term.ToLower() == "fuck")
                {
                    isValid = true;
                    break;
                }
            }

            return isValid;
        }

        private bool validateBlockedSites()
        {
            bool isValid = false;

            foreach (string site in blockedSites)
            {
                if (site.ToLower() == "pussy.org")
                {
                    isValid = true;
                    break;
                }
            }

            return isValid;
        }

        private bool validateFoundBlockedTerms()
        {
            bool isValid = true;

            //foreach (string term in foundBlockedTerms)
            //{
            //    if (term.ToLower().Contains("fuck"))
            //    {
            //        isValid = true;
            //        break;
            //    }
            //}

            return isValid;
        }

        #region datagridview events
        private void dgBlockedTerms_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            //make sure values are valid
            //DataGridView dg = (DataGridView)sender;

        }

        private void dgBlockedTerms_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //update blocked terms file
                ArrayList terms = new ArrayList();
                string value = "";
                DataGridView dg = (DataGridView)sender;
                foreach (DataGridViewRow row in dg.Rows)
                {
                    value = Convert.ToString(row.Cells["Terms"].Value);
                    if (value != null && value.Trim().Length > 0)
                        terms.Add(value);
                }

                blockedTerms = (string[])terms.ToArray(typeof(string));

                //encrypt
                blockedTerms = Encryption.Encrypt(blockedTerms);

                //save blockedTerms
                File.WriteAllLines(blockedTermsPath, blockedTerms);
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        private void logHeader(string path)
        {
            if (startTime.CompareTo(File.GetLastWriteTime(path)) == 1)
            {
                StringBuilder content = new StringBuilder();

                content.AppendLine();
                content.AppendLine("User: " + userName + "  Start Time: " + startTime);

                File.AppendAllText(path, Encryption.Encrypt(content.ToString()));
            }
        }

        public void log(string path, string content)
        {
            //make sure the path exists
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            logHeader(path);

            File.AppendAllText(path, content);
        }

        public void log(string path, string[] content)
        {
            //make sure the path exists
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            logHeader(path);

            File.WriteAllLines(path, content);
        }

        private void tcAdmin_VisibleChanged(object sender, EventArgs e)
        {
            closeToolStripButton.Visible = true;
        }

        private void loginTimer_Tick(object sender, EventArgs e)
        {
            loggedInTime++;

            if (loggedInTime > loginMaxTime)
            {
                loginTimer.Enabled = false;
                LoggedIn = false;
            }
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            string[] loginInfo = { "username:" + txtNewUserName.Text.Trim(), "password:" + txtNewPassword.Text.Trim() };
            loginInfo = Encryption.Encrypt(loginInfo);
            File.WriteAllLines(MainForm.loginInfoPath, loginInfo);
            lblLoginInfoUpdated.Visible = true;
        }

        private void tpChangeLoginInfo_Leave(object sender, EventArgs e)
        {
            lblLoginInfoUpdated.Visible = false;
        }

        private string getDefaultBrowser()
        {
            //original value on classesroot
            //"C:\Program Files\Internet Explorer\IEXPLORE.EXE" -nohome

            string browser = string.Empty;
            RegistryKey key = null;
            try
            {
                key = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", true);

                //trim off quotes
                //browser = key.GetValue(null).ToString().Replace("\"", "");
                //if (!browser.EndsWith(".exe"))
                //{
                //    //get rid of everything after the ".exe"
                //    browser = browser.Substring(0, browser.ToLower().LastIndexOf(".exe") + 4);
                //}

                browser = key.GetValue(null).ToString();

                //key.SetValue(null, (string)@browser);

                string safeSurfBrowser = "\"" + Application.ExecutablePath + "\"";

                key.SetValue(null, (string)@safeSurfBrowser);
            }
            finally
            {
                if (key != null) key.Close();
            }
            return browser;
        }

        private void JustinRecordtoolStripButton_Click(object sender, EventArgs e)
        {
            //need to get channel name from url
            string[] urlSegments = _windowManager.ActiveBrowser.Url.Segments;

            if (urlSegments[1].ToLower() != "directory")//this is a channel
            {
                string channelName = urlSegments[1];
                DialogResult result = MessageBox.Show("Are you sure you want to download from " + channelName, "Download " + channelName, MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    //pop up justin downloader and start downloading
                    //SCTVJustinTV.Downloader downloader = new SCTVJustinTV.Downloader(channelName, "12", Application.StartupPath + "\\JustinDownloads\\");
                    //SCTVJustinTV.Downloader downloader = new SCTVJustinTV.Downloader();
                    //downloader.Channel = channelName;
                    //downloader.Show();
                }
            }
            else
                MessageBox.Show("You must be watching the channel you want to record");
        }

        private void toolStripButtonFavorites_Click(object sender, EventArgs e)
        {
            string url = "";

            //check for url
            if (_windowManager.ActiveBrowser != null && _windowManager.ActiveBrowser.Url.PathAndQuery.Length > 0)
            {
                url = _windowManager.ActiveBrowser.Url.PathAndQuery;

                //add to onlineMedia.xml
                //SCTVObjects.MediaHandler.AddOnlineMedia(_windowManager.ActiveBrowser.Url.Host, _windowManager.ActiveBrowser.Url.PathAndQuery, "Online", "Favorites", "", "");
            }
            else
                MessageBox.Show("You must browse to a website to add it to your favorites");
        }

        private void showAddressBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _windowManager.ShowAddressBar = showAddressBarToolStripMenuItem.Checked;

            showAddressBarToolStripMenuItem.Checked = !showAddressBarToolStripMenuItem.Checked;
        }

        private string findValue(string stringToParse, string startPattern, string endPattern)
        {
            return findValue(stringToParse, startPattern, endPattern, false);
        }

        private string findValue(string stringToParse, string startPattern, string endPattern, bool returnSearchPatterns)
        {
            int start = 0;
            int end = 0;
            string foundValue = "";

            try
            {
                start = stringToParse.IndexOf(startPattern);

                if (start > -1)
                {
                    if (!returnSearchPatterns)
                        stringToParse = stringToParse.Substring(start + startPattern.Length);
                    else
                        stringToParse = stringToParse.Substring(start);

                    end = stringToParse.IndexOf(endPattern);

                    if (end > 0)
                    {
                        if (returnSearchPatterns)
                            foundValue = stringToParse.Substring(0, end + endPattern.Length);
                        else
                            foundValue = stringToParse.Substring(0, end);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
                //Tools.WriteToFile(ex);
            }

            return foundValue;
        }

        /// <summary>
        /// submit numbers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            foundSubmit = false;

            findSubmit(bitVideoBrowser.Document);
        }

        private void btnInstaGC_Click(object sender, EventArgs e)
        {
            //bitVideoBrowser.Url = new Uri("https://www.instagc.com/earn/offertoro/clicks");
        }

        /// <summary>
        /// quick pick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFaucetHub_Click(object sender, EventArgs e)
        {
            foundQuickPick = false;
            foundSubmit = false;

            findQuickPick(bitVideoBrowser.Document);
        }

        private void chbAutoRefresh_CheckedChanged(object sender, EventArgs e)
        {
            if (!chbAutoRefresh.Checked)
            {
                //secondsTimer.Tag = null;
                //secondsTimer.Stop();

                lblRefreshTimer.Text = "0 seconds";
            }
        }

        //[ComVisible(true)]
        //public class GetTourDocumentString
        //{
        //    public void CallServerSideCode()
        //    {
        //        try
        //        {
        //            MainForm currentForm = ((MainForm)Application.OpenForms[0]);

        //            var doc = currentForm.tourBrowser.Document;

        //            var renderedHtml = doc.GetElementsByTagName("HTML")[0].OuterHtml;

        //            currentForm.SetDocumentString = renderedHtml;
        //        }
        //        catch (Exception ex)
        //        {
        //            //Application.Restart();
        //        }
        //    }
        //}

        [ComVisible(true)]
        public class GetTourListDocumentString
        {
            public void CallServerSideCode()
            {
                try
                {
                    MainForm currentForm = ((MainForm)Application.OpenForms[0]);

                    var doc = currentForm.bitVideoBrowser.Document;

                    var renderedHtml = doc.GetElementsByTagName("HTML")[0].OuterHtml;

                    currentForm.SetTourListDocumentString = renderedHtml;
                }
                catch (Exception ex)
                {
                    //Application.Restart();
                }
            }
        }

        private void btnLoadSource_Click(object sender, EventArgs e)
        {
            tourIsRunning = false;
            tourList.Clear();
            //int.TryParse(txtStartingTourNum.Text, out currentTourIndex);

            currentTourIndex--;//get the index not the number

            if (currentTourIndex < 1)
                currentTourIndex = 1;

            //bitVideoBrowser.Navigate("javascript: window.external.CallServerSideCode();");
            string tempTourList = "";
            tempTourList = File.ReadAllText(Application.StartupPath + "\\tourlist.html");

            //documentLoaded_tourList(tempTourList);
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void btnLickey10_Click(object sender, EventArgs e)
        {
            refreshUtilities.Cancel();

            //check to see if we are already logged in
            if (currentUser.Length == 0)
                currentUser = getCurrentUser();

            //logout if another user is logged in
            if (currentUser.Trim().Length > 0 && currentUser.ToLower() != "lickey10@gmail.com")
            {
                userLoggingOut = currentUser;
                logout();
            }
            else
            {
                //login
                logIn("lickey10@gmail.com", "soccer");
            }
        }

        private void btnLickeyKids_Click(object sender, EventArgs e)
        {
            refreshUtilities.Cancel();

            //check to see if we are already logged in
            if (currentUser.Length == 0)
                currentUser = getCurrentUser();

            //logout if another user is logged in
            if (currentUser.Trim().Length > 0 && currentUser.ToLower() != "lickeykids@gmail.com")
            {
                userLoggingOut = currentUser;
                logout();
            }
            else
            {
                //login
                logIn("lickeykids@gmail.com", "soccer");
            }
        }

        private void btnSwitchUsers_Click(object sender, EventArgs e)
        {
            refreshUtilities.Cancel();

            //check to see if we are already logged in
            if (currentUser.Length == 0)
                currentUser = getCurrentUser();
            else
                users.Remove(users.Where(x => x.StartsWith(currentUser)).FirstOrDefault());

            //logout if another user is logged in
            //if (tempUser.Trim().Length > 0)
            //{
            if (users.Count > 0)
            {
                userLoggingOut = currentUser;

                logout();

                logBackIn = true;
            }
            else
            {
                currentUser = "";
                MessageBox.Show("All Done!!");

                log(statusLogPath, "******** All Done!! *******");

                log(finishedLogPath += "FINISHED_" + DateTime.Now.ToShortDateString().Replace("/", "") + "_" + DateTime.Now.ToShortTimeString().Replace(":", "") + ".txt", "******** All Done!! *******");
            }

            //}

            //if (users.Count > 0)
            //{
            //    //login
            //    foreach (string user in users)
            //    {
            //        if (userLoggingOut != user)
            //        {
            //            logIn(user.Split('|')[0], user.Split('|')[1]);

            //            break;
            //        }
            //    }
            //}
        }

        //private void btnCheckForButton_Click(object sender, EventArgs e)
        //{
        //    tourBrowser.Navigate("javascript: window.external.CallServerSideCode();");
        //}
    }
}