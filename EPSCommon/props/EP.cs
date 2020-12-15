using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.audionysos.general.props {

	/// <summary>Represents complete description of some parent's object property and is used as key element of extentend property concept.
	/// Instances of this class suppose to be static variables shared across multiple instances of <see cref="EP{T}"/> (produced by creating new instances of parent object).
	/// </summary>
	public sealed class EPInfo {
		/// <summary>Owner type where this property is defined.</summary>
		public readonly Type owner;
		/// <summary>Addres of the owner property used with reflection on owner type.</summary>
		public readonly string address;
		/// <summary>Short name of the propery.</summary>
		public readonly string name;
		/// <summary>Few words about the property to help end user when he don't know nomenclature.</summary>
		public readonly string info;
		/// <summary>Detailed description about the property for end user - how, where, when to use it, how it relates to other properites of a system etc.</summary>
		public readonly string description;
		/// <summary>Constrains used when setting extended property created using this info object.
		/// Any change of in this constrains will affect all instances of the property even if those changes were made after property instantiation.</summary>
		public readonly PConstrains constrains;
		/// <summary>Static inializer invoked when this instance where initialized.</summary>
		public readonly EPInitializer<EPInfo> initializer;
		/// <summary>Instance inializer invoked once after an instance of property this info desricbie is created.</summary>
		public readonly EPInitializer<EP> instanceInitializer;

		/// <summary>Creates new extented property info.</summary>
		/// <param name="owner">Type which owns described property.</param>
		/// <param name="address">Property name defined in owner type, usually aquired with <see cref="nameof"/> syntax.</param>
		/// <param name="staticInitializer">Static intializer invoked once after whole info abject is set.
		/// You can use existing initializers or wirte you own to perform any additional stuff needed when proprerty is define.</param>
		/// <param name="name">Name of the property displayed to frontend user. If name is not specified, property addres is used as a name.</param>
		/// <param name="info">Short info for frontend user about what this property represents.</param>
		/// <param name="description">Deatailed description about property usage.</param>
		/// <param name="constrains">Constrains object applayed each time an instance of the property is set to new value.</param>
		/// <param name="instanceInitializer">Intializer applayed each time new instance of the property is set.</param>
		public EPInfo(Type owner, string address, EPInitializer<EPInfo> staticInitializer = null, string name = null, string info = null, string description = null, PConstrains constrains = null, EPInitializer<EP> instanceInitializer = null) {
			this.owner = owner;
			this.address = address;
			this.name = name ?? address;
			this.description = description;
			this.constrains = constrains;
			this.instanceInitializer = instanceInitializer;
			this.initializer = staticInitializer;
			initializer?.initialize(this);
		}

		public override string ToString() {
			return $@"{owner.Name}.{address} [EPInfo]";
		}
	}

	/// <summary>Base class for generic extented property (see <see cref="EP{T}"/>) type.
	/// This class suppose to provide commont access point for code that can mange different type of <see cref="EP{T}"/>
	/// were working with explicit types is problematic or not possible. 
	/// </summary>
	public abstract class EP {
		/// <summary>Static detailed information about the property, used across multiple instances of property parent object.</summary>
		public abstract EPInfo info { get; }

		/// <summary>This sould return generic type of <see cref="EP{T}"/> instace.</summary>
		public abstract Type type { get; }

		/// <summary>The type of delegate that need to be passed to <see cref="listenEvent(Delegate, EPEvents)"/> and <see cref="muteEvent(Delegate, EPEvents)"/> methods.</summary>
		public abstract Type eventType { get; }

		/// <summary>If this instance is of <see cref="EP{T}"/> type an given argument are correct,
		/// this will create appropriate delegete from specfied generic method that can be later used as events handler.</summary>
		/// <param name="methodParent">Parent object of method with given name.</param>
		/// <param name="methodName">Name of generic method used to create the delegate.
		/// This method must take 1 generic type paramter T" and one argument of type <see cref="EPEvent{T}"/> which will be an event instance.</param>
		public Delegate createEventHandler(object methodParent, string methodName) {
			var propertyType = GetType().GenericTypeArguments[0];
			var mi = methodParent.GetType().GetMethod(methodName);
			mi = mi.MakeGenericMethod(propertyType);
			return Delegate.CreateDelegate(eventType, methodParent, mi);
		}

		/// <summary>Assigns event listener of specified event type.
		/// To stop listening events use <see cref="muteEvent(Delegate, EPEvents)"/> method.
		/// Those methods allow you to listen for <see cref="EP{T}"/> using <see cref="EP"/> base class, were T is "unknown".
		/// If you work on specfic type of <see cref="EP{T}"/>, use standard events aproach.</summary>
		/// <param name="l">Delegate to be invoked when event occurs. 
		/// This delegeate must by of <see cref="eventType"/> type. Use <see cref="createEventHandler(object, string)"/> to create delegate from any applicable method.
		/// Basically this is na <see cref="EPEventHandler{T}"/> where T is the type of extended property.</param>
		/// <param name="type">Type of event on wich to invoke given delegate.</param>
		public abstract void listenEvent(Delegate epEventHandler, EPEvents type);

		/// <summary>Remove event listener of specified event type.
		/// To start listening events use <see cref="listenEvent(Delegate, EPEvents)"/> method.
		/// Those methods allow you to listen for <see cref="EP{T}"/> using <see cref="EP"/> base class, were T is "unknown".
		/// If you work on specfic type of <see cref="EP{T}"/>, use standard events aproach.</summary>
		/// <param name="l">Delegate to be invoked when event occurs.
		/// This delegeate must by of <see cref="eventType"/> type.
		/// Basically this is na <see cref="EPEventHandler{T}"/> where T is the type of extended property.</param>
		/// <param name="type">Type of event on wich to invoke given delegate.</param>
		public abstract void muteEvent(Delegate ePEventHandler, EPEvents type);
	}

	/// <summary>Extended property is wrapper for objects of any type which should be accessed publicly or cared with some uniform manner.
	/// This class suppose to simplyfy process of designing complex properties of an object by providing single entry point for all common property settings,
	/// allowing to confidently create any constrians for any values and specify property dependencies at point of property delaration, specify description for frontend users,
	/// allow to track property changes using events and desing any other custom functionality shared across many prorperties.
	/// Main goals are to:
	/// -Reduce boilerplate to minimum.
	/// -Increase raliability of the program by minimzing amout of tedious, repetive progrmming task and provide uniform interface for defining any unusual progrmming patters.
	/// An instance of <see cref="EP{T}"/> represents a single instance of a property. Each new paren object sould have it's own <see cref="EP{T}"/> property instace.
	/// Extendet propery is basicaly compossed of two main objects - a <see cref="value"/> of extended property generic type and <see cref="info"/> which holds any reguired information to process the data.
	/// Because of this duality, an extendend property is created in two separte steps. First you create an instance of <see cref="EPInfo"/> describing the property, then you create instance of <see cref="EP{T}"/> providing that info in constructor.
	/// Info object of extended property is suppose to be static member of property's parent object an be shared accros multiple instance of the property (when new parent type instances are created),
	/// but an info is passed to <see cref="EP{T}"/> constructor each time new extended property is created so it is also possible to have different descriptions of the same property for different instances of parent type.
	/// The info object is not only intended to pssively describe some aspects of the property (as the name would suggest) like member Attributes does but it is an integral part of extendned property
	/// and in fact any fancy stuff in done from <see cref="EPInfo"/>.
	/// 
	/// Initializers
	/// Extended properties use two types of initializers to perform any additional opertions needed at property initialization.
	/// First is invoked only once after property info creation.
	/// Secound is Invoked each time after new instance of extended property is created with given info.
	/// Both initializer are basicaly te same abstract types deriving from <see cref="EPInitializer{T}"/>.
	/// Only diffrence is the argument which is passed to them at initialization times. First one take <see cref="EPInfo"/> as input and the other <see cref="EP{T}"/>.
	/// Those initializers are completely abstract and defined by programmer. Extended properties don't use any initializers by default.
	/// 
	/// Constrians
	/// Constrians define what happen to variable when you access it.
	/// Unlike initializers, constrains are used all the way acorss the lifespan of the property and are applied each time property is suppose to be set to new value or readed.
	/// Constrians can be grouped and aranged in any way and their exact behaviour and order of application is dependent on acutal constain's type. 
	/// Programmer can write and use he's own custom cosntrain and mix it with existing ones.
	/// Constrians can be very powerfull and it is possible to get practicaly any desired output value using them but thay should be used with care
	/// as thay can also as easly lead to unwanted restults and can impact on program preformance siginificantly.
	/// 
	/// When you create info for extented property, provided static initializer is ivoked
	/// </summary>
	public sealed class EP<T> : EP {

		/// <summary>Implements abstract method of <see cref="EP"/> base class.
		/// If you have acces to specific <see cref="EP{T}"/> type use normal event handling aproach.
		/// All events that could be accesible by this method here will be accessible publiclicly through <see cref="EP{Task}"/> API.
		/// See <see cref="EP.listenEvent(Delegate, EPEvents)"/> and <see cref="muteEvent(Delegate, EPEvents)"/> mothods description for more details.</summary>
		public override void listenEvent(Delegate l, EPEvents type){
			if (type == EPEvents.CHANGED) CHANGED += l as EPEventHandler<T>;
		}

		/// <summary>Implements abstract method of <see cref="EP"/> base class.
		/// If you have acces to specific <see cref="EP{T}"/> type use normal event handling aproach.
		/// All events that could be accesible by this method here will be accessible publiclicly through <see cref="EP{T}"/> API.
		/// See <see cref="EP.listenEvent(Delegate, EPEvents)"/> and <see cref="muteEvent(Delegate, EPEvents)"/> mothods description for more details.</summary>
		public override void muteEvent(Delegate l, EPEvents type) {
			if (type == EPEvents.CHANGED) CHANGED -= l as EPEventHandler<T>;
		}

		/// <summary>Dispatched when value of the property has changed.</summary>
		public event EPEventHandler<T> CHANGED;

		/// <summary>Current value of the property.</summary>
		private T _v;
		/// <summary>Current value of the property.</summary>
		public T value {
			get => _v;
			set {
				var i = _v;
				if(constrains) constrains.apply(value, ref _v);
				else _v = value;
				if (i != null && !i.Equals(_v)) CHANGED?.Invoke(new EPEvent<T>(this));
			}
		}

		/// <summary>Static detailed information about the property, used across multiple instances of property parent object.</summary>
		private EPInfo _info;
		/// <summary>Static detailed information about the property, used across multiple instances of property parent object.</summary>
		public override EPInfo info => _info;

		/// <summary>Generic type of this instance.</summary>
		public override Type type => typeof(T);
		/// <summary><see cref="EPEventHandler{T}"/> where T is the same generic type of this <see cref="EP{T}"/> instace.</summary>
		public override Type eventType => typeof(EPEventHandler<T>);

		/// <summary>Constrains used when setting this property.
		/// This is shortcut for "info.constrians" and is shared across multipe instances of the property.</summary>
		private PConstrains constrains => _info.constrains;

		/// <summary>Creates new extended property instance.</summary>
		/// <param name="value"></param>
		/// <param name="info"></param>
		public EP(T value, EPInfo info) {
			_info = info;
			this.value = value;
			if (constrains) constrains.CHANGED += onConstraninsChanged;
			info.instanceInitializer?.initialize(this);
		}

		private void onConstraninsChanged(PConstrainEvent e) => value = _v;

		/// <summary>Searches for instance intializer of specified type.
		/// If no initializer of specified type were found, null is returned.</summary>
		public I getInstanceIN<I>() where I : EPInitializer<EP> {
			return info.instanceInitializer?.getInitializer<I>();
		}

		/// <summary>Searches for static intializer of specified type.
		/// If no initializer of specified type were found, null is returned.</summary>
		public I getInitializer<I>() where I : EPInitializer<EPInfo> {
			return info.initializer?.getInitializer<I>();
		}

		#region Operators

		public static implicit operator T(EP<T> v) => v._v;

		public static bool operator >(EP<T> v1, EP<T> v2) {
			return (dynamic)v1 > (dynamic)v2;
		}

		public static bool operator <(EP<T> v1, EP<T> v2) {
			return (dynamic)v1 < (dynamic)v2;
		}

		public static bool operator ==(EP<T> v1, EP<T> v2) {
			return (dynamic)v1 == (dynamic)v2;
		}

		public static bool operator !=(EP<T> v1, EP<T> v2) {
			return (dynamic)v1 != (dynamic)v2;
		}

		public static EP<T> operator -(EP<T> v1, EP<T> v2) {
			return (dynamic)v1 != (dynamic)v2;
		}

		#endregion

		///</inheritdoc>
		public override string ToString() {
			return $@"{_v} [EP]";
		}
	}

	/// <summary>Lists event types fired by instances of <see cref="EP{T}"/>
	/// This is to provide valid event type input to <see cref="EP{T}.listenEvent(Delegate, EPEvents)"/> or <see cref="EP{T}.muteEvent(Delegate, EPEvents)"/> methods.</summary>
	public enum EPEvents {
		CHANGED
	}

	/// <summary>Delegate used to handle <see cref="EP{T}"/> events.</summary>
	/// <typeparam name="T">Instance of event that occured.</typeparam>
	public delegate void EPEventHandler<T>(EPEvent<T> e);

	/// <summary>Base, common class for <see cref="EPEvent{T}"/>.</summary>
	public abstract class EPEvent {
		internal abstract EP prop { get; } 
	}

	/// <summary>Represents any event rised by an <see cref="EP{T}"/> instance.</summary>
	public class EPEvent<T> : EPEvent {
		/// <summary>Property which created this event.</summary>
		public readonly EP<T> p;

		public EPEvent(EP<T> property) {
			this.p = property;
		}

		/// <summary>prop => p</summary>
		internal override EP prop => p;
	}
}
