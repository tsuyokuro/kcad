﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace KCad.Properties {
    using System;
    
    
    /// <summary>
    ///   ローカライズされた文字列などを検索するための、厳密に型指定されたリソース クラスです。
    /// </summary>
    // このクラスは StronglyTypedResourceBuilder クラスが ResGen
    // または Visual Studio のようなツールを使用して自動生成されました。
    // メンバーを追加または削除するには、.ResX ファイルを編集して、/str オプションと共に
    // ResGen を実行し直すか、または VS プロジェクトをビルドし直します。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   このクラスで使用されているキャッシュされた ResourceManager インスタンスを返します。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("KCad.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   厳密に型指定されたこのリソース クラスを使用して、すべての検索リソースに対し、
        ///   現在のスレッドの CurrentUICulture プロパティをオーバーライドします。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   import math
        ///
        ///x=0
        ///y=0
        ///z=0
        ///
        ///w=10
        ///h=10
        ///
        ///ratio=0.5
        ///
        ///def putMsg(s):
        ///    SE.PutMsg(s)
        ///
        ///def rect(w, h):
        ///    SE.Rect(w, h)
        ///
        ///def area():
        ///    SE.Area()
        ///
        ///def find(range):
        ///    SE.Find(range)
        ///
        ///def layerList():
        ///    SE.LayerList()
        ///
        ///def lastDown():
        ///    SE.ShowLastDownPoint()
        ///
        ///def distance():
        ///    SE.Distance()
        ///
        ///def group():
        ///    SE.Group()
        ///
        ///def ungroup():
        ///    SE.Ungroup()
        ///
        ///def addPoint():
        ///    SE.AddPoint()
        ///
        ///def addLayer(name):
        ///    SE.AddLayer(name)
        ///
        ///def reverse():
        ///    SE.ReverseOr [残りの文字列は切り詰められました]&quot;; に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string BaseScript {
            get {
                return ResourceManager.GetString("BaseScript", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Operation failed に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string error_operation_failed {
            get {
                return ResourceManager.GetString("error_operation_failed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Please select two or more object. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string error_select_2_or_more {
            get {
                return ResourceManager.GetString("error_select_2_or_more", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Please select 2 point に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string error_select_2_points {
            get {
                return ResourceManager.GetString("error_select_2_points", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Edit に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string menu_edit {
            get {
                return ResourceManager.GetString("menu_edit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   File に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string menu_file {
            get {
                return ResourceManager.GetString("menu_file", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Load に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string menu_load {
            get {
                return ResourceManager.GetString("menu_load", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Print に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string menu_print {
            get {
                return ResourceManager.GetString("menu_print", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Save に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string menu_save {
            get {
                return ResourceManager.GetString("menu_save", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Snap に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string menu_snap {
            get {
                return ResourceManager.GetString("menu_snap", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Operation success に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string notice_operation_success {
            get {
                return ResourceManager.GetString("notice_operation_success", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Objects was grouped に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string notice_was_grouped {
            get {
                return ResourceManager.GetString("notice_was_grouped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Objects was ungrouped に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string notice_was_ungrouped {
            get {
                return ResourceManager.GetString("notice_was_ungrouped", resourceCulture);
            }
        }
    }
}
