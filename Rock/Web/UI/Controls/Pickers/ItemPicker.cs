﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Base control that can be used to build treeview pickers
    /// </summary>
    public abstract class ItemPicker : CompositeControl, IRockControl, IRockChangeHandlerControl
    {
        #region IRockControl implementation

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the CSS Icon text.
        /// </summary>
        /// <value>
        /// The CSS icon class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string IconCssClass
        {
            get { return ViewState["IconCssClass"] as string ?? string.Empty; }
            set { ViewState["IconCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }

            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }

            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IRockControl" /> is required.
        /// </summary>
        /// <value><c>true</c> if required; otherwise, <c>false</c>.</value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get { return ViewState["Required"] as bool? ?? false; }
            set { ViewState["Required"] = value; }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }

            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                return RequiredFieldValidator.ValidationGroup;
            }

            set
            {
                RequiredFieldValidator.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Controls

        private HiddenFieldWithClass _hfItemId;
        private HiddenFieldWithClass _hfInitialItemParentIds;
        private HiddenFieldWithClass _hfExpandedCategoryIds;
        private HiddenFieldWithClass _hfItemName;
        private HiddenFieldWithClass _hfItemRestUrlExtraParams;

        private HtmlAnchor _btnSelect;
        private HtmlAnchor _btnSelectNone;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the control should be displayed Full-Width
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable full width]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableFullWidth { get; set; }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public abstract string ItemRestUrl { get; }

        /// <summary>
        /// Gets or sets the item rest URL extra params.
        /// </summary>
        /// <value>
        /// The item rest URL extra params.
        /// </value>
        public string ItemRestUrlExtraParams
        {
            get
            {
                EnsureChildControls();
                return _hfItemRestUrlExtraParams.Value;
            }

            set
            {
                EnsureChildControls();
                _hfItemRestUrlExtraParams.Value = value;
            }
        }

        /// <summary>
        /// Gets the item rest URL extra parameters control.
        /// </summary>
        /// <value>The item rest URL extra parameters control.</value>
        internal HiddenFieldWithClass ItemRestUrlExtraParamsControl
        {
            get
            {
                return _hfItemRestUrlExtraParams;
            }
        }

        /// <summary>
        /// Gets or sets the dropdown picker menu classes. Default: 'picker-menu dropdown-menu'
        /// </summary>
        /// <value>
        /// The CSS classes to apply.
        /// </value>
        public string PickerMenuCssClasses { get; set; }

        /// <summary>
        /// Gets or sets the item id.
        /// </summary>
        /// <value>
        /// The item id.
        /// </value>
        public string ItemId
        {
            get
            {
                EnsureChildControls();

                if ( IsItemIdEquivalentToEmpty( _hfItemId.Value ) )
                {
                    _hfItemId.Value = Constants.None.IdValue;
                }

                if ( UseCategorySelection )
                {
                    return _hfItemId.Value.Replace( CategoryPrefix, string.Empty );
                }

                return _hfItemId.Value;
            }

            set
            {
                EnsureChildControls();

                if ( IsItemIdEquivalentToEmpty( value ) )
                {
                    _hfItemId.Value = Constants.None.IdValue;
                }
                else
                {
                    if ( UseCategorySelection )
                    {
                        _hfItemId.Value = CategoryPrefix + value;
                    }
                    else
                    {
                        _hfItemId.Value = value;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a flag indicating if the provided value represents a reference to an empty item selection.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool IsItemIdEquivalentToEmpty( string value )
        {
            return ( string.IsNullOrWhiteSpace( value )
                     || value == Constants.None.IdValue
                     || ( this.UseCategorySelection && value == CategoryPrefix + Constants.None.IdValue ) );
        }

        /// <summary>
        /// Gets the item ids (including "0").
        /// NOTE: Make sure to exclude "0" when using this to get a list of actual selected items
        /// </summary>
        /// <value>
        /// The item ids.
        /// </value>
        public IEnumerable<string> ItemIds
        {
            get
            {
                EnsureChildControls();
                var ids = new List<string>();

                if ( !string.IsNullOrWhiteSpace( _hfItemId.Value ) )
                {
                    if ( UseCategorySelection )
                    {
                        ids.AddRange( _hfItemId.Value.Split( ',' ).Select( a => a.Replace( CategoryPrefix, string.Empty ) ) );
                    }
                    else
                    {
                        ids.AddRange( _hfItemId.Value.Split( ',' ) );
                    }
                }

                return ids;
            }

            set
            {
                EnsureChildControls();

                string newValue;

                if ( UseCategorySelection )
                {
                    newValue = string.Join( ",", value.Select( a => CategoryPrefix + a ) );
                }
                else
                {
                    newValue = string.Join( ",", value );
                }

                if ( IsItemIdEquivalentToEmpty( newValue ) )
                {
                    newValue = Constants.None.IdValue;
                }

                _hfItemId.Value = newValue;
            }
        }

        /// <summary>
        /// Gets or sets the initial item parent ids.  This should be a comma delimited list of ids of parents of
        /// selected items so that the tree view can expand them.  This should not be used for categories (only parent
        /// items of the same type, for example groups).
        /// </summary>
        /// <value>
        /// The initial item parent ids.
        /// </value>
        public virtual string InitialItemParentIds
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( _hfInitialItemParentIds.Value ) )
                {
                    _hfInitialItemParentIds.Value = Constants.None.IdValue;
                }

                return _hfInitialItemParentIds.Value;
            }

            set
            {
                EnsureChildControls();
                _hfInitialItemParentIds.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the expanded category ids.  This should be a comma delimited list of ids of categories that
        /// contain selected items so that the tree view can expand them.  For example, if a selected metric in a metric
        /// picker is underneath a category, this should contain the category id.
        /// </summary>
        /// <value>
        /// The expanded category ids.
        /// </value>
        public virtual string ExpandedCategoryIds
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( _hfExpandedCategoryIds.Value ) )
                {
                    _hfExpandedCategoryIds.Value = Constants.None.IdValue;
                }

                return _hfExpandedCategoryIds.Value;
            }

            set
            {
                EnsureChildControls();
                _hfExpandedCategoryIds.Value = value;
            }
        }

        /// <summary>
        /// Gets the selected value.
        /// </summary>
        /// <value>
        /// The selected value.  NOTE: If nothing was previously set, it will return <see cref="Rock.Constants.None.IdValue"/>.
        /// </value>
        public string SelectedValue
        {
            get
            {
                return ItemId;
            }

            private set
            {
                ItemId = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected values (including "0").
        /// NOTE: Make sure to exclude "0", or use <see cref="SelectedIds" />, when using this to get a list of actual selected items.
        /// </summary>
        /// <value>
        /// The selected values.
        /// </value>
        public IEnumerable<string> SelectedValues
        {
            get { return ItemIds; }
            private set { ItemIds = value; }
        }

        /// <summary>
        /// Gets or sets the name of the item.
        /// </summary>
        /// <value>
        /// The name of the item.
        /// </value>
        public string ItemName
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( _hfItemName.Value ) )
                {
                    _hfItemName.Value = !string.IsNullOrWhiteSpace( DefaultText ) ? DefaultText : Constants.None.TextHtml;
                }

                return _hfItemName.Value;
            }

            set
            {
                EnsureChildControls();
                _hfItemName.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the item names.
        /// </summary>
        /// <value>
        /// The item names.
        /// </value>
        public IEnumerable<string> ItemNames
        {
            get
            {
                EnsureChildControls();
                var names = new List<string>();

                if ( !string.IsNullOrWhiteSpace( _hfItemName.Value ) )
                {
                    names.AddRange( _hfItemName.Value.Split( ',' ) );
                }

                return names;
            }

            set
            {
                EnsureChildControls();
                _hfItemName.Value = string.Join( ",", value );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [allow multi select].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow multi select]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowMultiSelect { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow category selection].
        /// If set to true then the user will be allowed to select a Category in addition to the Items.
        /// Default value is false.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow category selection]; otherwise, <c>false</c>.
        /// </value>
        [RockObsolete( "1.11" )]
        [Obsolete( "ItemPicker no longer supports selection of both items and categories concurrently.", false )]
        public bool AllowCategorySelection
        {
            get => UseCategorySelection;
            set => UseCategorySelection = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether category selection is used.
        /// If set to true then the user will be allowed to select a Category but not Items.
        /// </summary>
        /// <value>
        ///   <c>true</c> if category selection is used; otherwise, <c>false</c>.
        /// </value>
        public bool UseCategorySelection { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether [show select children].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show select children]; otherwise, <c>false</c>.
        /// </value>
        internal bool ShowSelectChildren { get; set; }

        /// <summary>
        /// Gets or sets the default text.
        /// </summary>
        /// <value>
        /// The default text.
        /// </value>
        public string DefaultText { get; set; }

        /// <summary>
        /// Gets or sets the mode panel.
        /// </summary>
        /// <value>
        /// The mode panel.
        /// </value>
        public Panel ModePanel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show drop down].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show drop down]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowDropDown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [hide picker label].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide picker label]; otherwise, <c>false</c>.
        /// </value>
        public bool HidePickerLabel { get; set; }

        #endregion

        #region Fields

        /// <summary>
        /// The category prefix used when <see cref="UseCategorySelection"/> is true.
        /// </summary>
        public const string CategoryPrefix = "C";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemPicker" /> class.
        /// </summary>
        public ItemPicker()
            : base()
        {
            RequiredFieldValidator = new HiddenFieldValidator();
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            var sm = ScriptManager.GetCurrent( this.Page );
            EnsureChildControls();

            if ( sm != null )
            {
                sm.RegisterAsyncPostBackControl( _btnSelect );
                sm.RegisterAsyncPostBackControl( _btnSelectNone );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        /// <summary>
        /// Registers the java script.
        /// </summary>
        protected virtual void RegisterJavaScript()
        {
            string treeViewScript =
$@"Rock.controls.itemPicker.initialize({{ 
    controlId: '{this.ClientID}',
    restUrl: '{this.ResolveUrl( ItemRestUrl )}',
    allowMultiSelect: {this.AllowMultiSelect.ToString().ToLower()},
    allowCategorySelection: {this.UseCategorySelection.ToString().ToLower()},
    categoryPrefix: '{CategoryPrefix}',
    defaultText: '{this.DefaultText}',
    restParams: $('#{_hfItemRestUrlExtraParams.ClientID}').val(),
    expandedIds: [{this.InitialItemParentIds}],
    expandedCategoryIds: [{this.ExpandedCategoryIds}],
    showSelectChildren: {this.ShowSelectChildren.ToString().ToLower()}
}});
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "item_picker-treeviewscript_" + this.ClientID, treeViewScript, true );

            // Search Control

        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            _hfItemId = new HiddenFieldWithClass
            {
                ID = this.ID + "_hfItemId",
                CssClass = "js-item-id-value",
                Value = Constants.None.IdValue
            };

            _hfInitialItemParentIds = new HiddenFieldWithClass
            {
                ID = this.ID + "_hfInitialItemParentIds",
                CssClass = "js-initial-item-parent-ids-value",
                Value = Constants.None.IdValue
            };

            _hfExpandedCategoryIds = new HiddenFieldWithClass
            {
                ID = this.ID + "_hfExpandedCategoryIds",
                CssClass = "js-expanded-category-ids"
            };

            _hfItemName = new HiddenFieldWithClass
            {
                ID = this.ID + "_hfItemName",
                CssClass = "js-item-name-value"
            };

            _hfItemRestUrlExtraParams = new HiddenFieldWithClass
            {
                ID = this.ID + "_hfItemRestUrlExtraParams",
                CssClass = "js-item-rest-url-extra-params-value"
            };

            if ( ModePanel != null )
            {
                this.Controls.Add( ModePanel );
            }

            _btnSelect = new HtmlAnchor();
            _btnSelect.Attributes["class"] = "btn btn-xs btn-primary picker-btn";
            _btnSelect.ID = this.ID + "_btnSelect";
            _btnSelect.InnerText = "Select";
            _btnSelect.CausesValidation = false;

            // make sure  this always does a postback if this is a PagePicker or if ValueChanged is assigned, even if _selectItem is not assigned
            if ( _selectItem == null && ( this is PagePicker || _valueChanged != null ) )
            {
            _btnSelect.ServerClick += btnSelect_Click;
            }

            _btnSelectNone = new HtmlAnchor();
            _btnSelectNone.Attributes["class"] = "picker-select-none";
            _btnSelectNone.ID = this.ID + "_btnSelectNone";
            _btnSelectNone.InnerHtml = "<i class='fa fa-times'></i>";
            _btnSelectNone.CausesValidation = false;
            _btnSelectNone.Style[HtmlTextWriterStyle.Display] = "none";

            // make sure  this always does a postback if this is a PagePicker or if ValueChanged is assigned, even if _selectItem is not assigned
            if ( _selectItem == null && ( this is PagePicker || _valueChanged != null ) )
            {
                _btnSelectNone.ServerClick += btnSelect_Click;
            }

            Controls.Add( _hfItemId );
            Controls.Add( _hfInitialItemParentIds );
            Controls.Add( _hfExpandedCategoryIds );
            Controls.Add( _hfItemName );
            Controls.Add( _hfItemRestUrlExtraParams );
            Controls.Add( _btnSelect );
            Controls.Add( _btnSelectNone );

            RockControlHelper.CreateChildControls( this, Controls );

            RequiredFieldValidator.InitialValue = Constants.None.IdValue;
            RequiredFieldValidator.ControlToValidate = _hfItemId.ID;
            RequiredFieldValidator.Display = ValidatorDisplay.Dynamic;
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// This is where you implement the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public virtual void RenderBaseControl( HtmlTextWriter writer )
        {
            if ( this.Enabled )
            {
                writer.AddAttribute( "id", this.ClientID.ToString() );

                List<string> pickerClasses = new List<string>();
                pickerClasses.Add( "picker" );
                if ( EnableFullWidth )
                {
                    pickerClasses.Add( "picker-fullwidth" );
                }

                pickerClasses.Add( "picker-select rollover-container " + this.CssClass );

                writer.AddAttribute( "class", pickerClasses.AsDelimited( " " ) );

                foreach ( string styleKey in this.Style.Keys )
                {
                    writer.AddStyleAttribute( styleKey, this.Style[styleKey] );
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                _hfItemId.RenderControl( writer );
                _hfInitialItemParentIds.RenderControl( writer );
                _hfExpandedCategoryIds.RenderControl( writer );
                _hfItemName.RenderControl( writer );
                _hfItemRestUrlExtraParams.RenderControl( writer );

                if ( !HidePickerLabel )
                {
                    string pickerLabelHtmlFormat = @"
                    <a class='picker-label' href='#'>
                        <i class='{2} fa-fw'></i>
                        <span id='selectedItemLabel_{0}' class='selected-names'>{1}</span>
                        <b class='fa fa-caret-down pull-right'></b>
                    </a>";

                    writer.Write( pickerLabelHtmlFormat, this.ClientID, this.ItemName, this.IconCssClass );

                    writer.WriteLine();

                    _btnSelectNone.RenderControl( writer );
                }

                if ( string.IsNullOrEmpty( PickerMenuCssClasses ) )
                {
                    PickerMenuCssClasses = "picker-menu dropdown-menu";
                }

                // picker menu
                writer.AddAttribute( "class", PickerMenuCssClasses );
                if ( ShowDropDown )
                {
                    writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "block" );
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // mode panel
                if ( ModePanel != null )
                {
                    ModePanel.RenderControl( writer );
                }

                // treeview
                writer.Write(
                           @"<div id='treeview-scroll-container_{0}' class='scroll-container scroll-container-vertical scroll-container-picker'>
                                <div class='scrollbar'>
                                    <div class='track'>
                                        <div class='thumb'>
                                            <div class='end'></div>
                                        </div>
                                    </div>
                                </div>
                                <div id='treeview-view-port_{0}' class='viewport'>
                                    <div class='overview'>
                                        <div id='treeviewItems_{0}' class='treeview treeview-items'>               
                                        </div>
                                    </div>
                                </div>
                            </div>", this.ClientID );

                // picker actions
                writer.AddAttribute( "class", "picker-actions" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _btnSelect.RenderControl( writer );

                // render any additional picker actions that a child class if ItemPicker implements
                RenderCustomPickerActions( writer );

                writer.WriteLine();
                writer.RenderEndTag();

                // closing div of picker-menu
                writer.RenderEndTag();

                // closing div of picker
                writer.RenderEndTag();

                RegisterJavaScript();
            }
            else
            {
                // this picker is not enabled (readonly), so just render a readonly version
                List<string> pickerClasses = new List<string>();
                pickerClasses.Add( "picker" );

                if ( EnableFullWidth )
                {
                    pickerClasses.Add( "picker-fullwidth" );
                }

                pickerClasses.Add( "picker-select" );

                writer.AddAttribute( "class", pickerClasses.AsDelimited( " " ) );

                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                LinkButton linkButton = new LinkButton();
                linkButton.CssClass = "picker-label";
                linkButton.Text = string.Format( "<i class='{1}'></i><span>{0}</span>", this.ItemName, this.IconCssClass );
                linkButton.Enabled = false;
                linkButton.RenderControl( writer );
                writer.WriteLine();
                writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Render any additional picker actions
        /// </summary>
        /// <param name="writer">The writer.</param>
        public virtual void RenderCustomPickerActions( HtmlTextWriter writer )
        {
            writer.Write( "<a class='btn btn-xs btn-link picker-cancel' id='btnCancel_{0}'>Cancel</a>", this.ClientID );
        }

        /// <summary>
        /// Gets the selected value as an integer, returning NULL if the selected value is "0"
        /// </summary>
        /// <param name="noneAsNull">if set to <c>true</c> [none as null].</param>
        /// <returns></returns>
        /// <value>
        /// The selected value as int.
        /// </value>
        public int? SelectedValueAsInt( bool noneAsNull = true )
        {
            if ( string.IsNullOrWhiteSpace( ItemId ) )
            {
                return null;
            }

            int result = ItemId.AsInteger();
            if ( noneAsNull )
            {
                if ( result == Constants.None.Id )
                {
                    return null;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the value of the currently selected item.
        /// It will return NULL if either <see cref="T:Rock.Constants.None"/> or <see cref="T:Rock.Constants.All"/> is selected.
        /// </summary>
        /// <returns></returns>
        public int? SelectedValueAsId()
        {
            if ( string.IsNullOrWhiteSpace( ItemId ) )
            {
                return null;
            }

            int result = ItemId.AsInteger();
            if ( result == Constants.None.Id )
            {
                return null;
            }

            if ( result == Constants.All.Id )
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// Gets the selected ids (not including 0)
        /// </summary>
        /// <value>
        /// The selected ids.
        /// </value>
        public int[] SelectedIds => SelectedValuesAsInt().Where( a => a > 0 ).ToArray();

        /// <summary>
        /// Gets Selected numeric values as int (including 0).
        /// Note: In most cases, you should use <see cref="SelectedIds"/> instead.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> SelectedValuesAsInt()
        {
            var ids = new List<int>();

            if ( ItemIds == null || !ItemIds.Any() )
            {
                return ids;
            }

            foreach ( string keyVal in ItemIds )
            {
                int id;

                if ( int.TryParse( keyVal, out id ) )
                {
                    ids.Add( id );
                }
            }

            return ids;
        }

        /// <summary>
        /// Handles the Click event of the _btnSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSelect_Click( object sender, EventArgs e )
        {
            if ( this.AllowMultiSelect )
            {
                SetValuesOnSelect();
            }
            else
            {
                SetValueOnSelect();
            }

            _selectItem?.Invoke( sender, e );
            _valueChanged?.Invoke( sender, e );
        }

        /// <summary>
        /// Sets the selected value to the item with the specified id
        /// </summary>
        /// <param name="id">The id.</param>
        public void SetValue( int? id )
        {
            ItemId = id.HasValue ? id.Value.ToString() : Constants.None.IdValue;
            SetValueOnSelect();
        }

        /// <summary>
        /// Sets the selected values to the items with the specified ids
        /// </summary>
        /// <param name="ids">The ids.</param>
        public void SetValues( IEnumerable<int> ids )
        {
            ItemIds = ids != null ? ids.Select( i => i.ToString() ) : new List<string>();
            SetValuesOnSelect();
        }

        /// <summary>
        /// Sets the value on select.
        /// </summary>
        protected abstract void SetValueOnSelect();

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected abstract void SetValuesOnSelect();

        /// <summary>
        /// private reference to ValueChanged so that we can do special stuff in the add/remove accessors
        /// </summary>
        private event EventHandler _valueChanged;

        /// <summary>
        /// Occurs when the selected value has changed
        /// </summary>
        public event EventHandler ValueChanged
        {
            add
            {
                EnsureChildControls();
                _valueChanged += value;
                _btnSelect.ServerClick += btnSelect_Click;
                _btnSelectNone.ServerClick += btnSelect_Click;
            }

            remove
            {
                _valueChanged -= value;
                _btnSelect.ServerClick -= btnSelect_Click;
                _btnSelectNone.ServerClick -= btnSelect_Click;
            }
        }

        /// <summary>
        /// private reference to SelectItem so that we can do special stuff in the add/remove accessors
        /// </summary>
        private event EventHandler _selectItem;

        /// <summary>
        /// Occurs when [select item].
        /// </summary>
        public event EventHandler SelectItem
        {
            add
            {
                EnsureChildControls();
                _selectItem += value;
                _btnSelect.ServerClick += btnSelect_Click;
                _btnSelectNone.ServerClick += btnSelect_Click;
            }

            remove
            {
                _selectItem -= value;
                _btnSelect.ServerClick -= btnSelect_Click;
                _btnSelectNone.ServerClick -= btnSelect_Click;
            }
        }

        /// <summary>
        /// Shows the error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public void ShowErrorMessage( string errorMessage )
        {
            RequiredFieldValidator.ErrorMessage = errorMessage;
            RequiredFieldValidator.IsValid = false;
        }

        #endregion
    }
}