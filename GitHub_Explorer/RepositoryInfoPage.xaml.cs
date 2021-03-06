﻿using GitHub_Explorer.Common;
using GitHub_Explorer.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Octokit;
using Octokit.Helpers;
using Octokit.Internal;
using Octokit.Reflection;
using GitHub_Explorer.Service;
using GitHub_Explorer.NavigationParam;
using Windows.System.Threading;
using Newtonsoft.Json;
using Windows.UI.ViewManagement;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkID=390556 を参照してください

namespace GitHub_Explorer
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class RepositoryInfoPage : Page
    {
        private const string IssuesGroupName = "IssueGroup";
        private const string SecondGroupName = "SecondGroup";

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();

        private RepositoryInfoNaviParam naviParam;

        public RepositoryInfoPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// この <see cref="Page"/> に関連付けられた <see cref="NavigationHelper"/> を取得します。
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// この <see cref="Page"/> のビュー モデルを取得します。
        /// これは厳密に型指定されたビュー モデルに変更できます。
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// このページには、移動中に渡されるコンテンツを設定します。前のセッションからページを
        /// 再作成する場合は、保存状態も指定されます。
        /// </summary>
        /// <param name="sender">
        /// イベントのソース (通常、<see cref="NavigationHelper"/>)。
        /// </param>
        /// <param name="e">このページが最初に要求されたときに
        /// <see cref="Frame.Navigate(Type, Object)"/> に渡されたナビゲーション パラメーターと、
        /// 前のセッションでこのページによって保存された状態の辞書を提供する
        /// イベント データ。ページに初めてアクセスするとき、状態は null になります。</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// アプリケーションが中断される場合、またはページがナビゲーション キャッシュから破棄される場合、
        /// このページに関連付けられた状態を保存します。値は、
        /// <see cref="SuspensionManager.SessionState"/> のシリアル化の要件に準拠する必要があります。
        /// </summary>
        /// <param name="sender">イベントのソース (通常、<see cref="NavigationHelper"/>)。</param>
        /// <param name="e">シリアル化可能な状態で作成される空のディクショナリを提供するイベント データ
        ///。</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: ページの一意の状態をここに保存します。
        }

        /// <summary>
        /// このページがフレームに表示されるときに呼び出されます。
        /// </summary>
        /// <param name="e">このページにどのように到達したかを説明するイベント データ。
        /// このプロパティは、通常、ページを構成するために使用します。</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            string jsonParam = e.Parameter as string;
            naviParam = JsonConvert.DeserializeObject<RepositoryInfoNaviParam>(jsonParam);
            pivot.Title = naviParam.Name;
            await LoadRepositoryInfo(naviParam.Owner, naviParam.Name);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        private async Task LoadRepositoryInfo(string owner, string name)
        {
            try
            {
                statusBar.ProgressIndicator.Text = "Fetching repository infomation...";
                statusBar.ProgressIndicator.ShowAsync();
                var IssuesDataGroup = await IssueDataSource.GetGroupAsync(resourceLoader.GetString("PivotGroupIdIssues"), owner, name);
                this.DefaultViewModel[IssuesGroupName] = IssuesDataGroup;
            }
            catch(Exception e)
            { }
            finally
            {
                statusBar.ProgressIndicator.HideAsync();
            }
            return;
        }

        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // 適切な移動先のページに移動し、新しいページを構成します。
            // このとき、必要な情報をナビゲーション パラメーターとして渡します
            IssueInfoNaviParam objectParam = new IssueInfoNaviParam(naviParam.Owner, naviParam.Name, ((IssueDataItem)e.ClickedItem).Number);
            string jsonParam = JsonConvert.SerializeObject(objectParam);

            if (!Frame.Navigate(typeof(IssueInfoPage), jsonParam))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }
    }
}
