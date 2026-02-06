using System.Collections.Generic;
using UnityEngine;

namespace ShoelaceStudios.Utilities.SerializeInterfaces.Example
{
    public class ExampleInterfaceUser : MonoBehaviour
    {

        [RequireInterface(typeof(IExampleInterface))]
        public MonoBehaviour ExampleMonoWithRequire;
        public InterfaceReference<IExampleInterface> example;

        public InterfaceReference<IExampleInterface>[] exampleArray;
        public List<InterfaceReference<IExampleInterface>> exampleList;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Debug.Log(example.Value.GetBarkDebug());
        }
    }

    public interface IExampleInterface
    {
        public string GetBarkDebug();
    }
}
