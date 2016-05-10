﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ZSCY_Win10.Data.Community;
using ZSCY_Win10.Service;
using ZSCY_Win10.Util;
using ZSCY_Win10.ViewModels.Community;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ZSCY_Win10.Pages.CommunityPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CommunityMyContentPage : Page
    {
        CommunityMyContentViewModel ViewModel;
        List<Img> clickImgList = new List<Img>();
        bool isMark2Peo = false;//是否有回复某人
        ApplicationDataContainer appSetting = Windows.Storage.ApplicationData.Current.LocalSettings;
        ObservableCollection<Mark> markList = new ObservableCollection<Mark>();
        NavigationEventArgs ee;
        int clickImfIndex = 0;
        int remarkPage = 0;
        double oldmarkScrollViewerOffset = 0;
        bool isfirst = true;


        public CommunityMyContentPage()
        {
            this.InitializeComponent();
            this.SizeChanged += (s, e) =>
            {
                if (Utils.getPhoneWidth() > 800)
                {
                    CommunityItemPhotoGrid.Margin = new Thickness(-400, 0, 0, 0);
                }
                else
                {
                    CommunityItemPhotoGrid.Margin = new Thickness(0);
                }
            };
        }

        private void PhotoGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            Img img = e.ClickedItem as Img;
            GridView gridView = sender as GridView;
            clickImgList = ((Img[])gridView.ItemsSource).ToList();
            clickImfIndex = clickImgList.IndexOf(img);
            CommunityItemPhotoFlipView.ItemsSource = clickImgList;
            CommunityItemPhotoFlipView.SelectedIndex = clickImfIndex;
            if (Utils.getPhoneWidth() > 800)
            {
                CommunityItemPhotoGrid.Margin = new Thickness(-400, 0, 0, 0);
            }
            CommunityItemPhotoGrid.Visibility = Visibility.Visible;
            App.isPerInfoContentImgShow = true;
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void CommunityItemPhoto_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CommunityItemPhotoGrid.Visibility = Visibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= App_BackRequested;
            //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (CommunityItemPhotoGrid.Visibility == Visibility.Visible)
            {
                e.Handled = true;
                CommunityItemPhotoGrid.Visibility = Visibility.Collapsed;
                SystemNavigationManager.GetForCurrentView().BackRequested -= App_BackRequested;
                App.isPerInfoContentImgShow = false;
                //if (Utils.getPhoneWidth() > 800)
                //{

                //    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                //}
                //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }
        }

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {

        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ee = e;
            ViewModel = new CommunityMyContentViewModel(e.Parameter);
            base.OnNavigatedTo(e);
            markListView.ItemsSource = markList;
            getMark();
        }

        private async void getMark()
        {
            string id = "";
            string type_id = "";
            if (ViewModel.Item != null)
            {
                id = ViewModel.Item.id;
                type_id = ViewModel.Item.type_id;
            }
            else
            {
                type_id = "5";
                id = (ee.Parameter as MyNotification).article_id;
            }



            //if (ViewModel.BBDD != null)
            //{
            //    id = ViewModel.BBDD.id;
            //    type_id = ViewModel.BBDD.type_id;
            //}
            //else
            //{
            //    id = ViewModel.hotfeed.article_id;
            //    type_id = ViewModel.hotfeed.type_id;
            //}
            List<KeyValuePair<String, String>> paramList = new List<KeyValuePair<String, String>>();
            paramList.Add(new KeyValuePair<string, string>("article_id", id));
            paramList.Add(new KeyValuePair<string, string>("type_id", type_id));
            paramList.Add(new KeyValuePair<string, string>("stuNum", appSetting.Values["stuNum"].ToString()));
            paramList.Add(new KeyValuePair<string, string>("idNum", appSetting.Values["idNum"].ToString()));
            paramList.Add(new KeyValuePair<string, string>("size", "15"));
            paramList.Add(new KeyValuePair<string, string>("page", remarkPage.ToString()));
            string mark = await NetWork.getHttpWebRequest("cyxbsMobile/index.php/Home/ArticleRemark/getremark", paramList);
            Debug.WriteLine(mark);
            try
            {
                if (mark != "")
                {
                    JObject obj = JObject.Parse(mark);
                    if (Int32.Parse(obj["state"].ToString()) == 200)
                    {
                        //markList.Clear();
                        JArray markListArray = Utils.ReadJso(mark);
                        if (markListArray.Count != 0)
                        {
                            isfirst = false;
                            NoMarkGrid.Visibility = Visibility.Collapsed;
                            if (ViewModel.Item != null)
                            {
                                //ViewModel.Item.remark_num = markListArray.Count.ToString();
                                if (type_id == "5")
                                {
                                    MyFeed x = await CommunityMyContentService.GetFeed(int.Parse(type_id), id);
                                    MyFeed y=ee.Parameter as MyFeed;
                                    y.remark_num = x.remark_num;
                                }
                            }
                            //if (args is HotFeed)
                            //{
                            //    HotFeed h = args as HotFeed;
                            //    h.remark_num = ViewModel.BBDD.remark_num;
                            //}
                            for (int i = 0; i < markListArray.Count; i++)
                            {
                                Mark Markitem = new Mark();
                                Markitem.GetListAttribute((JObject)markListArray[i]);
                                markList.Add(Markitem);
                            }
                            remarkPage++;
                        }
                        else if (isfirst)
                        {
                            NoMarkGrid.Visibility = Visibility.Visible;
                        }

                    }
                }
            }
            catch (Exception) { }
            isMark2Peo = false;
        }

        private void sendMarkTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sendMarkTextBox.Text.Length > 0)
            {
                sendMarkButton.IsEnabled = true;
            }
            else
            {
                sendMarkButton.IsEnabled = false;
            }
        }

        private async void sendMarkButton_Click(object sender, RoutedEventArgs e)
        {
            sendMarkButton.IsEnabled = false;
            sendMarkProgressRing.Visibility = Visibility.Visible;
            string id = "";
            string type_id = "";
            if (ViewModel.Item != null)
            {
                id = ViewModel.Item.id;
                type_id = ViewModel.Item.type_id;
            }
            else
            {
                type_id = "5";
                id = (ee.Parameter as MyNotification).article_id;
            }
            List<KeyValuePair<String, String>> paramList = new List<KeyValuePair<String, String>>();
            paramList.Add(new KeyValuePair<string, string>("article_id", id));
            paramList.Add(new KeyValuePair<string, string>("type_id", type_id));
            paramList.Add(new KeyValuePair<string, string>("stuNum", appSetting.Values["stuNum"].ToString()));
            paramList.Add(new KeyValuePair<string, string>("idNum", appSetting.Values["idNum"].ToString()));
            paramList.Add(new KeyValuePair<string, string>("content", sendMarkTextBox.Text));
            string sendMark = await NetWork.getHttpWebRequest("cyxbsMobile/index.php/Home/ArticleRemark/postremarks", paramList);
            Debug.WriteLine(sendMark);
            try
            {
                if (sendMark != "")
                {
                    JObject obj = JObject.Parse(sendMark);
                    if (Int32.Parse(obj["state"].ToString()) == 200)
                    {
                        Utils.Toast("评论成功");
                        markList.Clear();
                        remarkPage = 0;
                        sendMarkTextBox.Text = "";
                        getMark();
                    }
                    else
                    {
                        Utils.Toast("评论失败");
                    }
                }
                else
                {
                    Utils.Toast("评论失败");
                }
                sendMarkProgressRing.Visibility = Visibility.Collapsed;
            }
            catch (Exception) { }
        }

        private void markListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickMarkItem = (Mark)e.ClickedItem;
            if (!isMark2Peo)
            {
                isMark2Peo = true;
            }
            else
            {
                sendMarkTextBox.Text = sendMarkTextBox.Text.Substring(sendMarkTextBox.Text.IndexOf(":") + 2);
            }
            sendMarkTextBox.Text = "回复 " + clickMarkItem.nickname + " : " + sendMarkTextBox.Text;
            sendMarkTextBox.Focus(FocusState.Keyboard);
            sendMarkTextBox.SelectionStart = sendMarkTextBox.Text.Length;
        }

        private void contentScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (contentScrollViewer.VerticalOffset > (contentScrollViewer.ScrollableHeight - 200) && contentScrollViewer.ScrollableHeight != oldmarkScrollViewerOffset)
            {
                oldmarkScrollViewerOffset = contentScrollViewer.ScrollableHeight;
                Debug.WriteLine("mark继续加载");
                getMark();
            }
        }
    }
}
