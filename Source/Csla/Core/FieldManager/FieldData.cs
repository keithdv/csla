//-----------------------------------------------------------------------
// <copyright file="FieldData.cs" company="Marimer LLC">
//     Copyright (c) Marimer LLC. All rights reserved.
//     Website: https://cslanet.com
// </copyright>
// <summary>Contains a field value and related metadata.</summary>
//-----------------------------------------------------------------------

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Csla.Serialization.Mobile;

namespace Csla.Core.FieldManager
{
  /// <summary>
  /// Contains a field value and related metadata.
  /// </summary>
  /// <typeparam name="T">Type of field value contained.</typeparam>
  [Serializable]
  public class FieldData<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    T> : IFieldData<T>
  {
    private T _data;
    private bool _isDirty;

    /// <summary>
    /// Creates a new instance of the object.
    /// </summary>
    public FieldData()
    {
      IsSerializable = true;
    }

    /// <summary>
    /// Creates a new instance of the object.
    /// </summary>
    /// <param name="name">
    /// Name of the field.
    /// </param>
    /// <param name="isSerializable">If property is serializable</param>
    public FieldData(string name, bool isSerializable)
    {
      Name = name;
      IsSerializable = isSerializable;
    }

    /// <summary>
    /// Gets the name of the field.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets or sets the value of the field.
    /// </summary>
    public virtual T Value
    {
      get
      {
        return _data;
      }
      set
      {
        _data = value;
        _isDirty = true;
      }
    }

    object IFieldData.Value
    {
      get
      {
        return Value;
      }
      set
      {
        if (value == null)
          Value = default(T);
        else
          Value = (T)value;
      }
    }
    /// <summary>
    /// Gets a value indicating whether this field
    /// references a serializable property.
    /// </summary>
    public bool IsSerializable { get; private set; }

    bool ITrackStatus.IsDeleted
    {
      get
      {
        if (_data is ITrackStatus child)
          return child.IsDeleted;
        else
          return false;
      }
    }

    bool ITrackStatus.IsSavable
    {
      get { return true; }
    }

    bool ITrackStatus.IsChild
    {
      get
      {
        if (_data is ITrackStatus child)
          return child.IsChild;
        else
          return false;
      }
    }

    /// <summary>
    /// Gets a value indicating whether the field
    /// has been changed.
    /// </summary>
    public virtual bool IsSelfDirty
    {
      get { return IsDirty; }
    }

    /// <summary>
    /// Gets a value indicating whether the field
    /// has been changed.
    /// </summary>
    public virtual bool IsDirty
    {
      get
      {
        if (_data is ITrackStatus child)
          return child.IsDirty;
        else
          return _isDirty;
      }
    }

    /// <summary>
    /// Marks the field as unchanged.
    /// </summary>
    public virtual void MarkClean()
    {
      _isDirty = false;
    }

    bool ITrackStatus.IsNew
    {
      get
      {
        if (_data is ITrackStatus child)
          return child.IsNew;
        else
          return false;
      }
    }

    bool ITrackStatus.IsSelfValid
    {
      get { return IsValid; }
    }

    bool ITrackStatus.IsValid
    {
      get { return IsValid; }
    }

    /// <summary>
    /// Gets a value indicating whether this field
    /// is considered valid.
    /// </summary>
    protected virtual bool IsValid
    {
      get
      {
        if (_data is ITrackStatus child)
          return child.IsValid;
        else
          return true;
      }
    }

    event BusyChangedEventHandler INotifyBusy.BusyChanged
    {
      add { throw new NotImplementedException(); }
      remove { throw new NotImplementedException(); }
    }

    /// <summary>
    /// Gets a value indicating whether this object or
    /// any of its child objects are busy.
    /// </summary>
    [Browsable(false)]
    [Display(AutoGenerateField = false)]
    [ScaffoldColumn(false)]
    public bool IsBusy
    {
      get
      {
        bool isBusy = false;
        if (_data is ITrackStatus child)
          isBusy = child.IsBusy;

        return isBusy;
      }
    }

    bool INotifyBusy.IsSelfBusy
    {
      get { return IsBusy; }
    }

    T IFieldData<T>.Value { get => Value; set => Value = value; }

    string IFieldData.Name => Name;

    bool ITrackStatus.IsDirty => IsDirty;

    bool ITrackStatus.IsSelfDirty => IsDirty;

    bool INotifyBusy.IsBusy => IsBusy;

    [NotUndoable]
    [NonSerialized]
    private EventHandler<ErrorEventArgs> _unhandledAsyncException;

    /// <summary>
    /// Event indicating that an exception occurred on
    /// a background thread.
    /// </summary>
    public event EventHandler<ErrorEventArgs> UnhandledAsyncException
    {
      add { _unhandledAsyncException = (EventHandler<ErrorEventArgs>)Delegate.Combine(_unhandledAsyncException, value); }
      remove { _unhandledAsyncException = (EventHandler<ErrorEventArgs>)Delegate.Remove(_unhandledAsyncException, value); }
    }

    event EventHandler<ErrorEventArgs> INotifyUnhandledAsyncException.UnhandledAsyncException
    {
      add { _unhandledAsyncException = (EventHandler<ErrorEventArgs>)Delegate.Combine(_unhandledAsyncException, value); }
      remove { _unhandledAsyncException = (EventHandler<ErrorEventArgs>)Delegate.Remove(_unhandledAsyncException, value); }
    }

    void IFieldData.MarkClean()
    {
      MarkClean();
    }

    void IMobileObject.GetState(SerializationInfo info)
    { }

    void IMobileObject.GetChildren(SerializationInfo info, MobileFormatter formatter)
    {
      bool isSerializable = formatter.IsTypeSerializable(_data.GetType());
      info.AddValue("_name", Name);
      info.AddValue("_isDataSerializable", isSerializable);
      if (isSerializable)
      {
        SerializationInfo childInfo = formatter.SerializeObject(_data);
        info.AddChild(Name, childInfo.ReferenceId, _isDirty);
      }
      else
      {
        info.AddValue("_data", _data);
        info.AddValue("_isDirty", _isDirty);
      }
    }

    void IMobileObject.SetState(SerializationInfo info)
    { }

    void IMobileObject.SetChildren(SerializationInfo info, MobileFormatter formatter)
    {
      Name = info.GetValue<string>("_name");
      bool isSerializable = info.GetValue<bool>("_isDataSerializable");
      if (isSerializable)
      {
        SerializationInfo.ChildData childData = info.Children[Name];
        _data = (T)formatter.GetObject(childData.ReferenceId);
      }
      else
      {
        _data = info.GetValue<T>("_data");
        _isDirty = info.GetValue<bool>("_isDirty");
      }
    }
  }
}
