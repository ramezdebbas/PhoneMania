using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace nivax.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : nivax.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            
            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
            String ITEM_CONTENT = String.Format("Item Content: {0}",
                        "nivax");

            var group1 = new SampleDataGroup("Group-1",
                    "Windows Phone",
                    "",
                    "Assets/10.png",
                    "");
            group1.Items.Add(new SampleDataItem("Group-1-Item-1",
                    "HTC Windows Phone 8X",
                    "",
                    "Assets/11.png",
                    "",
                    "I had the chance to hold this device in New York a few weeks ago, but was unable to test out the Windows Phone 8 OS at all since Microsoft still has a lockdown on showing it off until next week. The hardware and design is FANTASTIC though and if I was just interested in the coolest looking and feeling product then my decision would be easy. I think my wife and oldest daughter will be getting the 8X on T-Mobile as soon as they are released since they like good design and feel while services and high end geeky features don't mean as much to them. Here are my current thoughts on the HTC Windows Phone 8X:\n\nExcellent design and feel in the hand\nBrilliant color options\nSolid camera technology with useful ultra-wide angle front facing camera (self portraits with friends and family are very popular with my crowd)\nAvailability likely on the four major carriers\nAttractive 4.3 inch super LCD 2 display\nBeats Audio with integrated amp\nMinimal HTC services, basic Windows Phone 8 experience",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-2",
                    "HTC Windows Phone 8S",
                    "",
                    "Assets/12.png",
                    "",
                    "I personally will buy the highest end model of Windows Phone 8 from HTC or Nokia, but the 8S is a device for others to consider too. I do like the multi-color casing scheme and if design and color were my main considerations I may try to get one of these. Here are my thoughts on the HTC Windows Phone 8S:\n\nAttractive multi-color shell\nReplaceable microSD storage card\nBeats Audio",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-3",
                    "Nokia Lumia 920",
                    "",
                    "Assets/13.png",
                    "",
                    "I have the Nokia Lumia 900 now and the services I have, when compared to other Windows Phone devices, are of great value to me and the ability to get that on the 920 is very attractive. Nokia's cool new touchscreen technology, with support for stylus and gloves, looks to be very useful as winter approaches. Here are my current thoughts that have me with the Nokia Lumia 920 right now at the top of my list:\n\nDisplay technology\nNokia Maps, including turn-by-turn and offline navigation\nWireless charging\nPureView camera with Carl Zeiss optics has been shown to stand out above others\nFREE Nokia Music service\nOther exclusive Nokia apps are useful and worth value\nAT&T exclusive is NOT a good thing",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-4",
                    "Nokia Lumia 820",
                    "",
                    "Assets/14.png",
                    "",
                    "I won't be getting the Lumia 820/810 for myself, but the 810 is going to be on T-Mobile so I am considering it as an option over the 8X for my wife and oldest daughter. The 820/810 actually has some features that may sway people to get this over the 920, notwithstanding the AT&T exclusivity. Here are my current thoughts on the Nokia Lumia 820 and Lumia 810:\n\nSmaller 4.3 inch ClearBlack display with lower resolution\nFREE Nokia Music service and Nokia Maps\nReplaceable cover with ability for wireless charging\nRemovable battery\nReplaceable microSD storage card",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-5",
                    "Samsung Ativ S",
                    "",
                    "Assets/15.png",
                    "",
                    "The Samsung Galaxy S III is a fantastic Android smartphone and Samsung is doing the simple thing and basically taking this hardware and putting in Windows Phone 8, which is a strategy most manufacturers took the first time with Windows Phone 7. That isn't necessarily a bad thing though since the GSIII is great. Here are my thoughts on the Ativ S:\n\nLarge 4.8 inch HD Super AMOLED display\nSlick design with solid camera experience\nLarge 2,300 mAh battery\nExpandable memory via microSD card slot\n\nFor my personal choice, I am going to go with a device on AT&T and have it be either the HTC 8X or the Nokia Lumia 920. I have my iPhone 5 on Verizon and new Samsung Galaxy Note II on T-Mobile so have no plans to add Windows Phone 8 on those carriers. As you can see above, it primarily comes down to services or design so please give me your feedback and recommendations to consider as there may be more I haven't thought about. I will also try to get some hands-on time with these devices to make an even more informed decision as well.",
                    group1));
            this.AllGroups.Add(group1);

            var group2 = new SampleDataGroup("Group-2",
                    "iPhone",
                    "",
                    "Assets/20.png",
                    "");
            group2.Items.Add(new SampleDataItem("Group-2-Item-1",
                    "iPhone 5",
                    "",
                    "Assets/21.png",
                    "",
                    "The iPhone 5 is a touchscreen-based smartphone developed by Apple Inc.. It is the sixth generation of the iPhone and succeeds the iPhone 4S. The phone is a slimmer, lighter model that introduces a higher-resolution, 4-inch screen to the series with 16:9 widescreen aspect ratio. The phone also includes a custom-designed ARMv7 processor called the Apple A6, an update to Apple's mobile operating system known as iOS 6, and support for LTE.\n\nApple held an event to formally introduce the phone on September 12, 2012. Apple began taking pre-orders on September 14, 2012, and over two million were received within 24 hours. Initial demand for the iPhone 5 exceeded the supply available at launch on September 21, 2012, and has been described by Apple as extraordinary, with pre-orders having sold twenty times faster than its predecessors. Following the launch, Samsung filed a lawsuit against Apple, claiming that the iPhone 5 infringes eight of its patents.\n\nWhile reception to the iPhone 5 has been generally positive, the new Maps application featured on iOS 6 was negatively received and was reported to contain many serious errors. Consumers and reviewers have noted hardware issues, such as an unintended purple hue in photos taken by the iPhone 5, the phone's coating being prone to chipping, and the presence of light leaks on white variants of the device. Incompatibilities with LTE networks in some regions have also been noted.",
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-2",
                    "iPhone 4S",
                    "",
                    "Assets/22.png",
                    "",
                    "The iPhone 4S is a touchscreen-based smartphone developed by Apple Inc. It is the fifth generation of the iPhone, succeeding the iPhone 4, and was announced on October 4, 2011. It became available for pre-order on October 7, 2011 in seven initial countries (United States, Canada, Australia, United Kingdom, France, Germany and Japan) with the first delivery date set for October 14, 2011, and available on that same day for direct in-store sales in those countries. It was released in 22 more countries, including Ireland, Mexico, and Singapore, on October 28.\n\nThe phone retains the exterior design of its predecessor, but hosts improved hardware specifications and software updates. It added a voice recognition system known as Siri from which the 4S designator came, and a cloud storage service named iCloud. Some of the device's functions may be voice-controlled through Siri. The phone is available for 100 cell service carriers in 70 countries, including eight carriers in the United States. For US customers, unlocked (contract-free) sales started on November 11, 2011. The Associated Press said that AT&T described early iPhone 4S demand as extraordinary. Reception to the iPhone 4S was generally favorable. Reviewers noted Siri, the new camera, and processing speeds as significant advantages over the prior model. Four million units of the iPhone 4S were sold in the first three days of release.",
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-3",
                    "iPhone 4",
                    "",
                    "Assets/23.png",
                    "",
                    "The iPhone 4 is a touchscreen 3G smartphone developed by Apple Inc. It is the fourth generation iPhone, and successor to the iPhone 3GS. It is particularly marketed for video calling (marketed by Apple as FaceTime), consumption of media such as books and periodicals, movies, music, and games, and for general web and e-mail access. It was announced on June 7, 2010, at the WWDC 2010 held at the Moscone Center, San Francisco, and was released on June 24, 2010, in the United States, the United Kingdom, France, Germany and Japan.\n\nThe iPhone 4 runs Apple's iOS operating system, the same operating system as used on prior iPhones, the iPad, and the iPod Touch. It is mainly controlled by a user's fingertips on the multi-touch display, which is sensitive to fingertip contact. It originally shipped with iOS 4. The latest version available is iOS 6.1.3 (March 2013).",
                    group2));
            this.AllGroups.Add(group2);

            var group3 = new SampleDataGroup("Group-3",
                    "Android Phone",
                    "",
                    "Assets/30.png",
                    "");
            group3.Items.Add(new SampleDataItem("Group-3-Item-1",
                    "Huawei Ascend P2",
                    "",
                    "Assets/31.png",
                    "",
                    "Huawei Ascend P2 is one of the latest entrant in the quad core mobile phones segment from the Chinese manufacturer Huawei. The  phone comes with Quad-core 1.5 GHz processor running on Huawei K3V2 chipset and supports 1 GB Internal Ram (although a 2gb RAM in this phone would have certainly given higher spot in our ranking to this device). The phone has brilliant build and is one of the slimmest and lightest mobile phones in the list with just 8.44 mm thin. The phone has 4.7 inch display which supports 720 x 1280 pixels and 312 ppi pixel density and has Corning Gorilla Glass 2 Protection. The Phone would have Li-Ion 2420 mAh battery which would give phone enough juice to last one day easily.  Huawei Ascend P2 would come with Android 4.1 Jelly Bean Preinstalled and there is no information if the phone can be upgraded to Androd V4.2 yet. The phone has very decent 13 MP Primary camera and 1.3  MP Secondary camera. The Phone is expected to be available in April 2013 and we can expect the phone to be priced well.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-2",
                    "HTC Butterfly",
                    "",
                    "Assets/32.png",
                    "",
                    "HTC Butterfly is another very beautiful and powerful device here in the list. The Phone comes with Quad-core 1.5 GHz Krait processor and Adreno 320 GPU and has 2GB Ram. One thing about the phone that stands out is it’s very sharp and brilliant display, the phone has 5 inch Super LCD3 HD capacitive touchscreen which supports 1080 x 1920 pixels and 441 ppi pixel density and has Corning Gorilla Glass 2 Protection. One thing about the phone that misses it’s mark is it’s underpowered battery, we are not sure if a Non-removable Li-Po 2020 mAh battery would be able to support such a power hungry phone well throughout a full day. The phone comes with 8 MP primary camera and 2.1 MP secondary camera and comes with Android OS V4.1 preinstalled which would be upgradeable to Android 4.2. The phone also seems to be overpriced with unlocked version retailing for $800 is definitely costly.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-3",
                    "HTC One X",
                    "",
                    "Assets/33.png",
                    "",
                    "HTC’s ex Flagship phone HTC One X has definitely won a lot of accolades for it’ absolute lag free performance. HTC One X runs on Quad-core 1.5 GHz processor with ULP GeForce and Nvidia Tegra 3 chipset. HTC One X also comes with 1 GB Internal RAM and 16 GB/32 GB Internal Memory which can be expanded using an external SD Card. The phone has 4. 7 Inch Super IPS LCD2 capacitive touchscreen which supports 720 x 1280 pixels and 312 ppi pixel density. The phone comes with outdated Android OS V4.0 preinstalled which can be upgraded to Android OS V4.1 (but we can’t really complain here considering the phone is already a year old). It also has 8 MP Primary and 2 MP Secondary camera. The phone has Non-removable Li-Po 1800 mAh battery which is again pretty bad for a quad core   phone. The unlocked version of the phone is currently retailing for around $600.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-4",
                    "Samsung Galaxy Note II",
                    "",
                    "Assets/34.png",
                    "",
                    "Samsung Galaxy Note II is one of the highest selling android phone’s in the market thanks to it’s brilliant build, awesome display and very powerful processor. The phone comes with Quad-core 1.6 GHz Cortex-A9  processor with Mali-400MP GPU and Exynos 4412 Quad chipset and it has 2 GB RAM. But one thing about the phone that stands out is it’s very big 5.5 inch Display which also makes it the phone with the largest display in our list of quadcore phones and it is also the only phone that comes with S Pen Stylus. The 5.5 inch Super AMOLED capacitive touchscreen supports 720 x 1280 pixels and has 267 ppi pixel density. It also has 8 MP primary camera and 2 MP secondary camera and comes with Android OS 4.1 preinstalled which can be upgraded to Android OS 4.2. Samsung Galaxy Note II also has very powerful Li-Ion 3100 mAh battery. The phone is currently retailing at $650-$700 which makes it one of the most value for money phones in the market.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-5",
                    "Samsung Galaxy S III",
                    "",
                    "Assets/35.png",
                    "",
                    "Samsung Galaxy S III is the world’s highest selling Android Phone yet and has been the last Samsung flagship device (before Samsung Galaxy SIV). The processor specs of Samsung Galaxy SIII is very similar to that of NoteII except that the phone display size just feels a bit more apt. Samsung Galaxy SIII Quad-core 1.4 GHz Cortex-A9 with Mali-400MP GPU and Exynos 4412 Quad chipset and has 1 GB Ram. It has  4.8 inches Super AMOLED capacitive touchscreen with 306 ppi pixel density. It also has 8 MP primary camera and 2 MP secondary camera and comes with Android OS 4.1 preinstalled which can be upgraded to Android OS 4.2. Galaxy S III comes with Li-Ion 2100 mAh battery which would work well to support phone for a day. The phone currently sells for less $600 for unlocked version which is great price for phone with such awesome specs.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-6",
                    "LG Nexus 4",
                    "",
                    "Assets/36.png",
                    "",
                    "LG Nexus 4 is the current Google Android flagship phone and is the perfect phone if you are looking to buy a non modified vanilla version of latest Android. LG Nexus is the perfect phone for developers because you always tend to get the upgrades on the phone quickly and can play around with developer options more freely. LG Nexus 4 runs Quad-core 1.5 GHz Krait processor with Adreno 320 GPU and Qualcomm APQ8064 Snapdragon chipset. The Phone has 4.7 inch True HD IPS Plus capacitive touchscreen which supports 768 x 1280 pixels and 318 ppi pixel density. The phone also has 2 GB Ram which would make the Vanilla version of Android extremely smooth. The phone comes with Android OS, v4.2 (Jelly Bean) which is  up gradable to v4.2.2 (Jelly Bean) also the phone has Non-removable Li-Po 2100 mAh battery. The unlocked version of phone currently retails for $550.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-7",
                    "LG Optimus G Pro",
                    "",
                    "Assets/37.png",
                    "",
                    "LG Optimus G Pro is the real big competition to Samsung Note lineup. Finally LG seems to be on track with Android lineup of mobile phones. LG Optimus G Pro has features to outclass any other quadcore smartphone from this list thanks to it’s powerful processor, sharp hd display, battery and camera. LG Optimus G Pro comes with Quad-core 1.7 GHz Krait 300 with Adreno 320 GPU and Qualcomm APQ8064T Snapdragon 600 chipset. Also the Phone has 2 GB RAM which makes it even more special. The phone has 5.5 inch True Full HD IPS LCD capacitive touchscreen which supports 1080 x 1920 pixels and 401 ppi pixel density. The phone has 13 MP primary camera and 2.1 MP Secondary camera. It comes with even more powerful Li-Po 3140 mAh battery. The phone runs on Android OS V4.1 (Jelly Bean) and is expected to be upgeadable to Android V4.2 in near future.",
                    group3));
            this.AllGroups.Add(group3);
        }
    }
}
