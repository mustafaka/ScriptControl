using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting;

/// <summary>
/// The real plug-in interface we use to communicate across app-domains
/// </summary>
public interface IRun
{
    void Initialize(IDictionary<string,object> Variables);
    object Run(string StartMethod, params object[] Parameters);
    void Dispose(IDictionary<string, object> Variables);
}

/// <summary>
/// Factory class to create objects exposing IRun
/// </summary>
public class ScriptInstance : MarshalByRefObject
    {
        private const BindingFlags bfi = BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance;

        public ScriptInstance() { }

	        /// <summary> Factory method to create an instance of the type whose name is specified,
	        /// using the named assembly file and the constructor that best matches the specified parameters. </summary>
	        /// <param name="assemblyFile"> The name of a file that contains an assembly where the type named typeName is sought. </param>
	        /// <param name="typeName"> The name of the preferred type. </param>
	        /// <param name="constructArgs"> An array of arguments that match in number, order, and type the parameters of the constructor to invoke, or null for default constructor. </param>
        /// <returns> The return value is the created object represented as IRun. </returns>
        public IRun Create(string assemblyFile, string typeName, object[] constructArgs)
        {
            return (IRun)Activator.CreateInstanceFrom(
            assemblyFile, typeName, false, bfi, null, constructArgs,
            null, null, null).Unwrap();
        }

        public IRun Create(AppDomain appdomain, string assemblyFile, string typeName, object[] constructArgs)
        {
            return (IRun)Activator.CreateInstanceFrom(appdomain,
            assemblyFile, typeName, false, bfi, null, constructArgs,
            null, null, null).Unwrap();
        }


        public IRun Create(Type type, object[] constructArgs)
        {

           ObjectHandle h = (ObjectHandle) Activator.CreateInstance(type, constructArgs);
           return (IRun)h.Unwrap();
        }
    }
   

