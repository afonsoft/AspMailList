﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AspMailList.Service
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="afonsoftcombr_db")]
	public partial class dbMalaDiretaDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertMala_Direta(Mala_Direta instance);
    partial void UpdateMala_Direta(Mala_Direta instance);
    partial void DeleteMala_Direta(Mala_Direta instance);
    partial void InsertMala_Direta_Campanha_Unsubscribe(Mala_Direta_Campanha_Unsubscribe instance);
    partial void UpdateMala_Direta_Campanha_Unsubscribe(Mala_Direta_Campanha_Unsubscribe instance);
    partial void DeleteMala_Direta_Campanha_Unsubscribe(Mala_Direta_Campanha_Unsubscribe instance);
    partial void InsertMala_Direta_Campanha(Mala_Direta_Campanha instance);
    partial void UpdateMala_Direta_Campanha(Mala_Direta_Campanha instance);
    partial void DeleteMala_Direta_Campanha(Mala_Direta_Campanha instance);
    partial void InsertMala_Direta_Campanha_Enviado(Mala_Direta_Campanha_Enviado instance);
    partial void UpdateMala_Direta_Campanha_Enviado(Mala_Direta_Campanha_Enviado instance);
    partial void DeleteMala_Direta_Campanha_Enviado(Mala_Direta_Campanha_Enviado instance);
    #endregion
		
		public dbMalaDiretaDataContext() : 
				base(global::AspMailList.Service.Properties.Settings.Default.afonsoftcombr_dbConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public dbMalaDiretaDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public dbMalaDiretaDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public dbMalaDiretaDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public dbMalaDiretaDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<Mala_Direta> Mala_Diretas
		{
			get
			{
				return this.GetTable<Mala_Direta>();
			}
		}
		
		public System.Data.Linq.Table<Mala_Direta_Campanha_Unsubscribe> Mala_Direta_Campanha_Unsubscribes
		{
			get
			{
				return this.GetTable<Mala_Direta_Campanha_Unsubscribe>();
			}
		}
		
		public System.Data.Linq.Table<Mala_Direta_Campanha> Mala_Direta_Campanhas
		{
			get
			{
				return this.GetTable<Mala_Direta_Campanha>();
			}
		}
		
		public System.Data.Linq.Table<Mala_Direta_Campanha_Enviado> Mala_Direta_Campanha_Enviados
		{
			get
			{
				return this.GetTable<Mala_Direta_Campanha_Enviado>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="afonsoftcombr.Mala_Direta")]
	public partial class Mala_Direta : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _id;
		
		private string _email;
		
		private System.DateTime _dtCadastro;
		
		private System.Nullable<System.DateTime> _dtExclusao;
		
		private EntitySet<Mala_Direta_Campanha_Unsubscribe> _Mala_Direta_Campanha_Unsubscribes;
		
		private EntitySet<Mala_Direta_Campanha_Enviado> _Mala_Direta_Campanha_Enviados;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidChanging(int value);
    partial void OnidChanged();
    partial void OnemailChanging(string value);
    partial void OnemailChanged();
    partial void OndtCadastroChanging(System.DateTime value);
    partial void OndtCadastroChanged();
    partial void OndtExclusaoChanging(System.Nullable<System.DateTime> value);
    partial void OndtExclusaoChanged();
    #endregion
		
		public Mala_Direta()
		{
			this._Mala_Direta_Campanha_Unsubscribes = new EntitySet<Mala_Direta_Campanha_Unsubscribe>(new Action<Mala_Direta_Campanha_Unsubscribe>(this.attach_Mala_Direta_Campanha_Unsubscribes), new Action<Mala_Direta_Campanha_Unsubscribe>(this.detach_Mala_Direta_Campanha_Unsubscribes));
			this._Mala_Direta_Campanha_Enviados = new EntitySet<Mala_Direta_Campanha_Enviado>(new Action<Mala_Direta_Campanha_Enviado>(this.attach_Mala_Direta_Campanha_Enviados), new Action<Mala_Direta_Campanha_Enviado>(this.detach_Mala_Direta_Campanha_Enviados));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_id", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int id
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((this._id != value))
				{
					this.OnidChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("id");
					this.OnidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_email", DbType="VarChar(500) NOT NULL", CanBeNull=false)]
		public string email
		{
			get
			{
				return this._email;
			}
			set
			{
				if ((this._email != value))
				{
					this.OnemailChanging(value);
					this.SendPropertyChanging();
					this._email = value;
					this.SendPropertyChanged("email");
					this.OnemailChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_dtCadastro", DbType="DateTime NOT NULL")]
		public System.DateTime dtCadastro
		{
			get
			{
				return this._dtCadastro;
			}
			set
			{
				if ((this._dtCadastro != value))
				{
					this.OndtCadastroChanging(value);
					this.SendPropertyChanging();
					this._dtCadastro = value;
					this.SendPropertyChanged("dtCadastro");
					this.OndtCadastroChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_dtExclusao", DbType="DateTime")]
		public System.Nullable<System.DateTime> dtExclusao
		{
			get
			{
				return this._dtExclusao;
			}
			set
			{
				if ((this._dtExclusao != value))
				{
					this.OndtExclusaoChanging(value);
					this.SendPropertyChanging();
					this._dtExclusao = value;
					this.SendPropertyChanged("dtExclusao");
					this.OndtExclusaoChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Mala_Direta_Mala_Direta_Campanha_Unsubscribe", Storage="_Mala_Direta_Campanha_Unsubscribes", ThisKey="id", OtherKey="idMail")]
		public EntitySet<Mala_Direta_Campanha_Unsubscribe> Mala_Direta_Campanha_Unsubscribes
		{
			get
			{
				return this._Mala_Direta_Campanha_Unsubscribes;
			}
			set
			{
				this._Mala_Direta_Campanha_Unsubscribes.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Mala_Direta_Mala_Direta_Campanha_Enviado", Storage="_Mala_Direta_Campanha_Enviados", ThisKey="id", OtherKey="idMail")]
		public EntitySet<Mala_Direta_Campanha_Enviado> Mala_Direta_Campanha_Enviados
		{
			get
			{
				return this._Mala_Direta_Campanha_Enviados;
			}
			set
			{
				this._Mala_Direta_Campanha_Enviados.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_Mala_Direta_Campanha_Unsubscribes(Mala_Direta_Campanha_Unsubscribe entity)
		{
			this.SendPropertyChanging();
			entity.Mala_Direta = this;
		}
		
		private void detach_Mala_Direta_Campanha_Unsubscribes(Mala_Direta_Campanha_Unsubscribe entity)
		{
			this.SendPropertyChanging();
			entity.Mala_Direta = null;
		}
		
		private void attach_Mala_Direta_Campanha_Enviados(Mala_Direta_Campanha_Enviado entity)
		{
			this.SendPropertyChanging();
			entity.Mala_Direta = this;
		}
		
		private void detach_Mala_Direta_Campanha_Enviados(Mala_Direta_Campanha_Enviado entity)
		{
			this.SendPropertyChanging();
			entity.Mala_Direta = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="afonsoftcombr.Mala_Direta_Campanha_Unsubscribe")]
	public partial class Mala_Direta_Campanha_Unsubscribe : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _id;
		
		private int _idMail;
		
		private int _idCampanha;
		
		private System.DateTime _dtUnsubscribe;
		
		private EntityRef<Mala_Direta> _Mala_Direta;
		
		private EntityRef<Mala_Direta_Campanha> _Mala_Direta_Campanha;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidChanging(int value);
    partial void OnidChanged();
    partial void OnidMailChanging(int value);
    partial void OnidMailChanged();
    partial void OnidCampanhaChanging(int value);
    partial void OnidCampanhaChanged();
    partial void OndtUnsubscribeChanging(System.DateTime value);
    partial void OndtUnsubscribeChanged();
    #endregion
		
		public Mala_Direta_Campanha_Unsubscribe()
		{
			this._Mala_Direta = default(EntityRef<Mala_Direta>);
			this._Mala_Direta_Campanha = default(EntityRef<Mala_Direta_Campanha>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_id", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int id
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((this._id != value))
				{
					this.OnidChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("id");
					this.OnidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_idMail", DbType="Int NOT NULL")]
		public int idMail
		{
			get
			{
				return this._idMail;
			}
			set
			{
				if ((this._idMail != value))
				{
					if (this._Mala_Direta.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnidMailChanging(value);
					this.SendPropertyChanging();
					this._idMail = value;
					this.SendPropertyChanged("idMail");
					this.OnidMailChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_idCampanha", DbType="Int NOT NULL")]
		public int idCampanha
		{
			get
			{
				return this._idCampanha;
			}
			set
			{
				if ((this._idCampanha != value))
				{
					if (this._Mala_Direta_Campanha.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnidCampanhaChanging(value);
					this.SendPropertyChanging();
					this._idCampanha = value;
					this.SendPropertyChanged("idCampanha");
					this.OnidCampanhaChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_dtUnsubscribe", DbType="DateTime NOT NULL")]
		public System.DateTime dtUnsubscribe
		{
			get
			{
				return this._dtUnsubscribe;
			}
			set
			{
				if ((this._dtUnsubscribe != value))
				{
					this.OndtUnsubscribeChanging(value);
					this.SendPropertyChanging();
					this._dtUnsubscribe = value;
					this.SendPropertyChanged("dtUnsubscribe");
					this.OndtUnsubscribeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Mala_Direta_Mala_Direta_Campanha_Unsubscribe", Storage="_Mala_Direta", ThisKey="idMail", OtherKey="id", IsForeignKey=true)]
		public Mala_Direta Mala_Direta
		{
			get
			{
				return this._Mala_Direta.Entity;
			}
			set
			{
				Mala_Direta previousValue = this._Mala_Direta.Entity;
				if (((previousValue != value) 
							|| (this._Mala_Direta.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._Mala_Direta.Entity = null;
						previousValue.Mala_Direta_Campanha_Unsubscribes.Remove(this);
					}
					this._Mala_Direta.Entity = value;
					if ((value != null))
					{
						value.Mala_Direta_Campanha_Unsubscribes.Add(this);
						this._idMail = value.id;
					}
					else
					{
						this._idMail = default(int);
					}
					this.SendPropertyChanged("Mala_Direta");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Mala_Direta_Campanha_Mala_Direta_Campanha_Unsubscribe", Storage="_Mala_Direta_Campanha", ThisKey="idCampanha", OtherKey="id", IsForeignKey=true)]
		public Mala_Direta_Campanha Mala_Direta_Campanha
		{
			get
			{
				return this._Mala_Direta_Campanha.Entity;
			}
			set
			{
				Mala_Direta_Campanha previousValue = this._Mala_Direta_Campanha.Entity;
				if (((previousValue != value) 
							|| (this._Mala_Direta_Campanha.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._Mala_Direta_Campanha.Entity = null;
						previousValue.Mala_Direta_Campanha_Unsubscribes.Remove(this);
					}
					this._Mala_Direta_Campanha.Entity = value;
					if ((value != null))
					{
						value.Mala_Direta_Campanha_Unsubscribes.Add(this);
						this._idCampanha = value.id;
					}
					else
					{
						this._idCampanha = default(int);
					}
					this.SendPropertyChanged("Mala_Direta_Campanha");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="afonsoftcombr.Mala_Direta_Campanha")]
	public partial class Mala_Direta_Campanha : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _id;
		
		private string _DisplayName;
		
		private string _ReplyTo;
		
		private string _From;
		
		private string _SmtpServer;
		
		private bool _EnableSsl;
		
		private int _SmtpPort;
		
		private int _PopPort;
		
		private string _SmtpUser;
		
		private string _SmtpPassword;
		
		private string _Subject;
		
		private string _BodyHtml;
		
		private string _AttachmentPath;
		
		private bool _Enabled;
		
		private EntitySet<Mala_Direta_Campanha_Unsubscribe> _Mala_Direta_Campanha_Unsubscribes;
		
		private EntitySet<Mala_Direta_Campanha_Enviado> _Mala_Direta_Campanha_Enviados;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidChanging(int value);
    partial void OnidChanged();
    partial void OnDisplayNameChanging(string value);
    partial void OnDisplayNameChanged();
    partial void OnReplyToChanging(string value);
    partial void OnReplyToChanged();
    partial void OnFromChanging(string value);
    partial void OnFromChanged();
    partial void OnSmtpServerChanging(string value);
    partial void OnSmtpServerChanged();
    partial void OnEnableSslChanging(bool value);
    partial void OnEnableSslChanged();
    partial void OnSmtpPortChanging(int value);
    partial void OnSmtpPortChanged();
    partial void OnPopPortChanging(int value);
    partial void OnPopPortChanged();
    partial void OnSmtpUserChanging(string value);
    partial void OnSmtpUserChanged();
    partial void OnSmtpPasswordChanging(string value);
    partial void OnSmtpPasswordChanged();
    partial void OnSubjectChanging(string value);
    partial void OnSubjectChanged();
    partial void OnBodyHtmlChanging(string value);
    partial void OnBodyHtmlChanged();
    partial void OnAttachmentPathChanging(string value);
    partial void OnAttachmentPathChanged();
    partial void OnEnabledChanging(bool value);
    partial void OnEnabledChanged();
    #endregion
		
		public Mala_Direta_Campanha()
		{
			this._Mala_Direta_Campanha_Unsubscribes = new EntitySet<Mala_Direta_Campanha_Unsubscribe>(new Action<Mala_Direta_Campanha_Unsubscribe>(this.attach_Mala_Direta_Campanha_Unsubscribes), new Action<Mala_Direta_Campanha_Unsubscribe>(this.detach_Mala_Direta_Campanha_Unsubscribes));
			this._Mala_Direta_Campanha_Enviados = new EntitySet<Mala_Direta_Campanha_Enviado>(new Action<Mala_Direta_Campanha_Enviado>(this.attach_Mala_Direta_Campanha_Enviados), new Action<Mala_Direta_Campanha_Enviado>(this.detach_Mala_Direta_Campanha_Enviados));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_id", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int id
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((this._id != value))
				{
					this.OnidChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("id");
					this.OnidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DisplayName", DbType="NVarChar(200) NOT NULL", CanBeNull=false)]
		public string DisplayName
		{
			get
			{
				return this._DisplayName;
			}
			set
			{
				if ((this._DisplayName != value))
				{
					this.OnDisplayNameChanging(value);
					this.SendPropertyChanging();
					this._DisplayName = value;
					this.SendPropertyChanged("DisplayName");
					this.OnDisplayNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ReplyTo", DbType="NVarChar(200) NOT NULL", CanBeNull=false)]
		public string ReplyTo
		{
			get
			{
				return this._ReplyTo;
			}
			set
			{
				if ((this._ReplyTo != value))
				{
					this.OnReplyToChanging(value);
					this.SendPropertyChanging();
					this._ReplyTo = value;
					this.SendPropertyChanged("ReplyTo");
					this.OnReplyToChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Name="[From]", Storage="_From", DbType="NVarChar(200) NOT NULL", CanBeNull=false)]
		public string From
		{
			get
			{
				return this._From;
			}
			set
			{
				if ((this._From != value))
				{
					this.OnFromChanging(value);
					this.SendPropertyChanging();
					this._From = value;
					this.SendPropertyChanged("From");
					this.OnFromChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SmtpServer", DbType="NVarChar(200) NOT NULL", CanBeNull=false)]
		public string SmtpServer
		{
			get
			{
				return this._SmtpServer;
			}
			set
			{
				if ((this._SmtpServer != value))
				{
					this.OnSmtpServerChanging(value);
					this.SendPropertyChanging();
					this._SmtpServer = value;
					this.SendPropertyChanged("SmtpServer");
					this.OnSmtpServerChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_EnableSsl", DbType="Bit NOT NULL")]
		public bool EnableSsl
		{
			get
			{
				return this._EnableSsl;
			}
			set
			{
				if ((this._EnableSsl != value))
				{
					this.OnEnableSslChanging(value);
					this.SendPropertyChanging();
					this._EnableSsl = value;
					this.SendPropertyChanged("EnableSsl");
					this.OnEnableSslChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SmtpPort", DbType="Int NOT NULL")]
		public int SmtpPort
		{
			get
			{
				return this._SmtpPort;
			}
			set
			{
				if ((this._SmtpPort != value))
				{
					this.OnSmtpPortChanging(value);
					this.SendPropertyChanging();
					this._SmtpPort = value;
					this.SendPropertyChanged("SmtpPort");
					this.OnSmtpPortChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PopPort", DbType="Int NOT NULL")]
		public int PopPort
		{
			get
			{
				return this._PopPort;
			}
			set
			{
				if ((this._PopPort != value))
				{
					this.OnPopPortChanging(value);
					this.SendPropertyChanging();
					this._PopPort = value;
					this.SendPropertyChanged("PopPort");
					this.OnPopPortChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SmtpUser", DbType="NVarChar(200) NOT NULL", CanBeNull=false)]
		public string SmtpUser
		{
			get
			{
				return this._SmtpUser;
			}
			set
			{
				if ((this._SmtpUser != value))
				{
					this.OnSmtpUserChanging(value);
					this.SendPropertyChanging();
					this._SmtpUser = value;
					this.SendPropertyChanged("SmtpUser");
					this.OnSmtpUserChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SmtpPassword", DbType="NVarChar(200) NOT NULL", CanBeNull=false)]
		public string SmtpPassword
		{
			get
			{
				return this._SmtpPassword;
			}
			set
			{
				if ((this._SmtpPassword != value))
				{
					this.OnSmtpPasswordChanging(value);
					this.SendPropertyChanging();
					this._SmtpPassword = value;
					this.SendPropertyChanged("SmtpPassword");
					this.OnSmtpPasswordChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Subject", DbType="NVarChar(200) NOT NULL", CanBeNull=false)]
		public string Subject
		{
			get
			{
				return this._Subject;
			}
			set
			{
				if ((this._Subject != value))
				{
					this.OnSubjectChanging(value);
					this.SendPropertyChanging();
					this._Subject = value;
					this.SendPropertyChanged("Subject");
					this.OnSubjectChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_BodyHtml", DbType="Text NOT NULL", CanBeNull=false, UpdateCheck=UpdateCheck.Never)]
		public string BodyHtml
		{
			get
			{
				return this._BodyHtml;
			}
			set
			{
				if ((this._BodyHtml != value))
				{
					this.OnBodyHtmlChanging(value);
					this.SendPropertyChanging();
					this._BodyHtml = value;
					this.SendPropertyChanged("BodyHtml");
					this.OnBodyHtmlChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_AttachmentPath", DbType="NVarChar(4000)")]
		public string AttachmentPath
		{
			get
			{
				return this._AttachmentPath;
			}
			set
			{
				if ((this._AttachmentPath != value))
				{
					this.OnAttachmentPathChanging(value);
					this.SendPropertyChanging();
					this._AttachmentPath = value;
					this.SendPropertyChanged("AttachmentPath");
					this.OnAttachmentPathChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Enabled", DbType="Bit NOT NULL")]
		public bool Enabled
		{
			get
			{
				return this._Enabled;
			}
			set
			{
				if ((this._Enabled != value))
				{
					this.OnEnabledChanging(value);
					this.SendPropertyChanging();
					this._Enabled = value;
					this.SendPropertyChanged("Enabled");
					this.OnEnabledChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Mala_Direta_Campanha_Mala_Direta_Campanha_Unsubscribe", Storage="_Mala_Direta_Campanha_Unsubscribes", ThisKey="id", OtherKey="idCampanha")]
		public EntitySet<Mala_Direta_Campanha_Unsubscribe> Mala_Direta_Campanha_Unsubscribes
		{
			get
			{
				return this._Mala_Direta_Campanha_Unsubscribes;
			}
			set
			{
				this._Mala_Direta_Campanha_Unsubscribes.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Mala_Direta_Campanha_Mala_Direta_Campanha_Enviado", Storage="_Mala_Direta_Campanha_Enviados", ThisKey="id", OtherKey="idCampanha")]
		public EntitySet<Mala_Direta_Campanha_Enviado> Mala_Direta_Campanha_Enviados
		{
			get
			{
				return this._Mala_Direta_Campanha_Enviados;
			}
			set
			{
				this._Mala_Direta_Campanha_Enviados.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_Mala_Direta_Campanha_Unsubscribes(Mala_Direta_Campanha_Unsubscribe entity)
		{
			this.SendPropertyChanging();
			entity.Mala_Direta_Campanha = this;
		}
		
		private void detach_Mala_Direta_Campanha_Unsubscribes(Mala_Direta_Campanha_Unsubscribe entity)
		{
			this.SendPropertyChanging();
			entity.Mala_Direta_Campanha = null;
		}
		
		private void attach_Mala_Direta_Campanha_Enviados(Mala_Direta_Campanha_Enviado entity)
		{
			this.SendPropertyChanging();
			entity.Mala_Direta_Campanha = this;
		}
		
		private void detach_Mala_Direta_Campanha_Enviados(Mala_Direta_Campanha_Enviado entity)
		{
			this.SendPropertyChanging();
			entity.Mala_Direta_Campanha = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="afonsoftcombr.Mala_Direta_Campanha_Enviado")]
	public partial class Mala_Direta_Campanha_Enviado : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _id;
		
		private int _idMail;
		
		private int _idCampanha;
		
		private System.DateTime _dtEnvio;
		
		private EntityRef<Mala_Direta> _Mala_Direta;
		
		private EntityRef<Mala_Direta_Campanha> _Mala_Direta_Campanha;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidChanging(int value);
    partial void OnidChanged();
    partial void OnidMailChanging(int value);
    partial void OnidMailChanged();
    partial void OnidCampanhaChanging(int value);
    partial void OnidCampanhaChanged();
    partial void OndtEnvioChanging(System.DateTime value);
    partial void OndtEnvioChanged();
    #endregion
		
		public Mala_Direta_Campanha_Enviado()
		{
			this._Mala_Direta = default(EntityRef<Mala_Direta>);
			this._Mala_Direta_Campanha = default(EntityRef<Mala_Direta_Campanha>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_id", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int id
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((this._id != value))
				{
					this.OnidChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("id");
					this.OnidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_idMail", DbType="Int NOT NULL")]
		public int idMail
		{
			get
			{
				return this._idMail;
			}
			set
			{
				if ((this._idMail != value))
				{
					if (this._Mala_Direta.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnidMailChanging(value);
					this.SendPropertyChanging();
					this._idMail = value;
					this.SendPropertyChanged("idMail");
					this.OnidMailChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_idCampanha", DbType="Int NOT NULL")]
		public int idCampanha
		{
			get
			{
				return this._idCampanha;
			}
			set
			{
				if ((this._idCampanha != value))
				{
					if (this._Mala_Direta_Campanha.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnidCampanhaChanging(value);
					this.SendPropertyChanging();
					this._idCampanha = value;
					this.SendPropertyChanged("idCampanha");
					this.OnidCampanhaChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_dtEnvio", DbType="DateTime NOT NULL")]
		public System.DateTime dtEnvio
		{
			get
			{
				return this._dtEnvio;
			}
			set
			{
				if ((this._dtEnvio != value))
				{
					this.OndtEnvioChanging(value);
					this.SendPropertyChanging();
					this._dtEnvio = value;
					this.SendPropertyChanged("dtEnvio");
					this.OndtEnvioChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Mala_Direta_Mala_Direta_Campanha_Enviado", Storage="_Mala_Direta", ThisKey="idMail", OtherKey="id", IsForeignKey=true)]
		public Mala_Direta Mala_Direta
		{
			get
			{
				return this._Mala_Direta.Entity;
			}
			set
			{
				Mala_Direta previousValue = this._Mala_Direta.Entity;
				if (((previousValue != value) 
							|| (this._Mala_Direta.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._Mala_Direta.Entity = null;
						previousValue.Mala_Direta_Campanha_Enviados.Remove(this);
					}
					this._Mala_Direta.Entity = value;
					if ((value != null))
					{
						value.Mala_Direta_Campanha_Enviados.Add(this);
						this._idMail = value.id;
					}
					else
					{
						this._idMail = default(int);
					}
					this.SendPropertyChanged("Mala_Direta");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Mala_Direta_Campanha_Mala_Direta_Campanha_Enviado", Storage="_Mala_Direta_Campanha", ThisKey="idCampanha", OtherKey="id", IsForeignKey=true)]
		public Mala_Direta_Campanha Mala_Direta_Campanha
		{
			get
			{
				return this._Mala_Direta_Campanha.Entity;
			}
			set
			{
				Mala_Direta_Campanha previousValue = this._Mala_Direta_Campanha.Entity;
				if (((previousValue != value) 
							|| (this._Mala_Direta_Campanha.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._Mala_Direta_Campanha.Entity = null;
						previousValue.Mala_Direta_Campanha_Enviados.Remove(this);
					}
					this._Mala_Direta_Campanha.Entity = value;
					if ((value != null))
					{
						value.Mala_Direta_Campanha_Enviados.Add(this);
						this._idCampanha = value.id;
					}
					else
					{
						this._idCampanha = default(int);
					}
					this.SendPropertyChanged("Mala_Direta_Campanha");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
#pragma warning restore 1591
