using System.Reflection;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace TheNorthernCastleStates;

public static class TNCSHelper
{
    private static PropertyInfo Accuracy = AccessTools.Property(typeof(WeaponComponentData), "Accuracy");
    private static PropertyInfo MissileSpeed = AccessTools.Property(typeof(WeaponComponentData), "MissileSpeed");
    private static PropertyInfo SwingSpeed = AccessTools.Property(typeof(WeaponComponentData), "SwingSpeed");
    private static PropertyInfo SwingDamage = AccessTools.Property(typeof(WeaponComponentData), "SwingDamage");
    private static PropertyInfo ThrustSpeed = AccessTools.Property(typeof(WeaponComponentData), "ThrustSpeed");
    private static PropertyInfo ThrustDamage = AccessTools.Property(typeof(WeaponComponentData), "ThrustDamage");
    private static PropertyInfo Handling = AccessTools.Property(typeof(WeaponComponentData), "Handling");
    
    public static void ModifyWeapons()
    {
        ItemObject itemObject = MBObjectManager.Instance.GetObject<ItemObject>("tncs_eastern_javelin_3_t6");
        MissileSpeed.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 30);
        Accuracy.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 94);
        
        itemObject = MBObjectManager.Instance.GetObject<ItemObject>("tncs_western_javelin_1_t2");
        MissileSpeed.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 32);
        Accuracy.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 91);
        
        itemObject = MBObjectManager.Instance.GetObject<ItemObject>("tncs_battania_noble_sword_1_t6");
        SwingSpeed.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 80);
        SwingDamage.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 105);
        ThrustSpeed.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 84);
        ThrustDamage.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 42);
        Handling.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 75);
        
        itemObject = MBObjectManager.Instance.GetObject<ItemObject>("tncs_battania_noble_sword_1_t6_2");
        SwingSpeed.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 86);
        SwingDamage.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 101);
        ThrustSpeed.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 92);
        ThrustDamage.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 50);
        Handling.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 80);
        
        itemObject = MBObjectManager.Instance.GetObject<ItemObject>("tncs_aserai_2haxe_3_t5");
        SwingSpeed.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 70);
        SwingDamage.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 152);
        Handling.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 64);
        
        itemObject = MBObjectManager.Instance.GetObject<ItemObject>("tncs_vlandia_2hsword_2_t5");
        SwingSpeed.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 100);
        SwingDamage.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 134);
        ThrustSpeed.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 98);
        ThrustDamage.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 59);
        Handling.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 80);
        
        itemObject = MBObjectManager.Instance.GetObject<ItemObject>("tncs_vlandia_2hsword_2_t5_2");
        SwingSpeed.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 86);
        SwingDamage.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 152);
        ThrustSpeed.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 87);
        ThrustDamage.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 67);
        Handling.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 75);
        
        itemObject = MBObjectManager.Instance.GetObject<ItemObject>("tncs_khuzait_lance_3_t5");
        ThrustSpeed.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 86);
        ThrustDamage.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 46);
        Handling.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 65);
        
        itemObject = MBObjectManager.Instance.GetObject<ItemObject>("tncs_sickle_blade_1");
        SwingSpeed.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 100);
        SwingDamage.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 68);
        Handling.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 98);
        
        itemObject = MBObjectManager.Instance.GetObject<ItemObject>("tncs_sickle_blade_2");
        SwingSpeed.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 97);
        SwingDamage.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 48);
        Handling.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 95);
        
        itemObject = MBObjectManager.Instance.GetObject<ItemObject>("tncs_sickle_blade_3");
        SwingSpeed.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 108);
        SwingDamage.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 82);
        Handling.SetValue(itemObject.WeaponComponent.PrimaryWeapon, 105);
    }
}