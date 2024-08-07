using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BBBirder.UnityVue;
using BBBirder.UnityInjection;

public class CollectionTest : IPrebuildSetup
{
    [Test]
    public void List_Add_Should_Emit_Count_And_Element()
    {
        var group = CSReactive.Reactive(new CubeGroup());
        var count = -1;
        var volume0 = 0f;
        CSReactive.WatchEffect(() =>
        {
            if (group.cubeList.Count > 0)
            {
                var c = group.cubeList[0];
                volume0 = c.Length * c.Width * c.Height;
            }
            else
            {
                volume0 = 0;
            }
        }).WithArguments(ScopeFlushMode.Immediate);

        CSReactive.Watch(
            () => group.cubeList.Count,
            c => count = c
        ).WithArguments(ScopeFlushMode.Immediate);

        var list = group.cubeList;
        var cubeA = group.cubeA;

        list.Add(group.cubeA);
        Assert.AreEqual(group.cubeA, list[0]);
        Assert.AreEqual(group.cubeList, list);
        Assert.AreEqual(1, list.Count);
        Assert.AreEqual(1, count);

        cubeA.Width = cubeA.Height = cubeA.Length = 2;
        Assert.AreEqual(8, volume0);

        Debug.Log(list);

        list.Insert(0, new()
        {
            Width = 2,
            Height = 3,
            Length = 7,
        });
        Assert.AreEqual(2, count);
        Assert.AreEqual(2 * 3 * 7, volume0);

        Debug.Log(list);
        list.RemoveAt(0);
        Debug.Log(list);
        Assert.AreEqual(1, count);
        Assert.AreEqual(2, list[0].Length);
        Assert.AreEqual(8, volume0);

        list.Clear();
        Assert.AreEqual(0, count);
        Assert.AreEqual(0, volume0);
    }

    [Test]
    public void List_AddRange_Should_Emit_Count_Elements()
    {
        var group = CSReactive.Reactive(new CubeGroup()
        {
            cubeList = new()
            {
                new(),
                new(),
                new(),
            }
        });
        var evts = new List<string>();
        // ((IWatchable)group).onPropertySet += evts.Add;
        var vSum = 0f;
        var count = 0;
        CSReactive.WatchEffect(() =>
        {
            var sum = 0f;
            foreach (var cube in group.cubeList)
            {
                sum += cube.Volume;
            }
            vSum = sum;
        }).WithArguments(ScopeFlushMode.Immediate);
        CSReactive.Watch(
            () => group.cubeList.Count,
            c => count = c
        ).WithArguments(ScopeFlushMode.Immediate);

        group.cubeList.AddRange(new Cube[] { new() { Volume = 1 }, new() { Volume = 2 } });
        Assert.AreEqual(5, group.cubeList.Count);
        Assert.AreEqual(5, count);
        Assert.AreEqual(3, vSum);
    }

    [Test]
    public void Dictionary_Count_Should_Emit_On_Change()
    {
        var dict = CSReactive.Reactive(new RDictionary<string, string>());
        var cnt = -1;
        CSReactive.WatchEffect(() =>
        {
            cnt = dict.Count;
        }).WithArguments(ScopeFlushMode.Immediate);
        Assert.AreEqual(0, cnt);
        dict["a"] = "a";
        Assert.AreEqual(1, cnt);
        dict["b"] = "b";
        Assert.AreEqual(2, cnt);
        dict["a"] = "b";
        Assert.AreEqual(2, cnt);
        dict.Add("c", "c");
        Assert.AreEqual(3, cnt);
        dict.Remove("a");
        Assert.AreEqual(2, cnt);
        dict.Clear();
        Assert.AreEqual(0, cnt);
        dict["Count"] = "111";
        Assert.AreEqual(1, cnt);
    }

    [Test]
    public void Dictionary_Item_Should_Emit_On_Change()
    {
        var dict = CSReactive.Reactive(new RDictionary<string, string>());
        var value = "";
        CSReactive.WatchEffect(() =>
        {
            if (!dict.TryGetValue("a", out value))
            {
                value = null;
            }
        }).WithArguments(ScopeFlushMode.Immediate);
        dict["a"] = "a";
        Assert.AreEqual("a", value);
        dict.Remove("a");
        Assert.AreEqual(null, value);
        dict.Add("a", "asd");
        Assert.AreEqual("asd", value);
        dict.Clear();
        Assert.AreEqual(null, value);
    }
    public void Setup()
    {
        InjectionDriver.Instance.InstallAllAssemblies();
    }
}
