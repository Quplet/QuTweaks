using Menu.Remix.MixedUI;

namespace QuTweaks
{
    public class QuTweaksOptions : OptionInterface
    {
        public readonly Configurable<bool> EasierTongueBreak;
        public readonly Configurable<bool> LessTriggerHappyDropwigs;
        public readonly Configurable<bool> HeavierSpears;

        public QuTweaksOptions()
        {
            EasierTongueBreak = config.Bind(nameof(EasierTongueBreak), true);
            LessTriggerHappyDropwigs = config.Bind(nameof(LessTriggerHappyDropwigs), true);
            HeavierSpears = config.Bind(nameof(HeavierSpears), false);
        }

        public override void Initialize()
        {
            base.Initialize();
            
            var opTab = new OpTab(this, "Options");
            Tabs = new[] { opTab };
            var titleBox = new OpLabel(16f, 560f, "Configuration", true);
            
            /*
            var easierTongueBreakBox = new OpCheckBox(EasierTongueBreak, 16f, 520f);
            easierTongueBreakBox.description = 
                "Makes it easier to break out of certain lizard tongue grips. Default is true.";
            var easierTongueBreakLabel = new OpLabel(easierTongueBreakBox.PosX + 40f, easierTongueBreakBox.PosY, "Easier lizard tongue break");
            easierTongueBreakLabel.description = easierTongueBreakBox.description;
            */

            var easierTongueBreakConfig = new QuConfigOption(new OpCheckBox(EasierTongueBreak, 16f, 520f), 
                "Easier lizard tongue break", 
                "Makes it easier to break out of certain lizard tongue grips. Default is true.");
            
            /*
            var lessTriggerHappyDropwigsBox = new OpCheckBox(LessTriggerHappyDropwigs, 16f, 480f);
            lessTriggerHappyDropwigsBox.description =
                "Makes dropwigs not instantly drop when you throw a spear at them. Default is true.";
            var lessTriggerHappyDropwigsLabel = new OpLabel(lessTriggerHappyDropwigsBox.PosX + 40f, lessTriggerHappyDropwigsBox.PosY, "Less trigger happy dropwigs");
            lessTriggerHappyDropwigsLabel.description = lessTriggerHappyDropwigsBox.description;
            */

            var lessTriggerHappyDropwigsConfig = new QuConfigOption(new OpCheckBox(LessTriggerHappyDropwigs, 16f, 480f),
                "Less trigger happy dropwigs",
                "Makes dropwigs not instantly drop when you throw a spear at them. Default is true.");

            /*
            var heavierSpearsBox = new OpCheckBox(HeavierSpears, 16f, 440f);
            heavierSpearsBox.description = 
                "Causes lizards to flip over upon being hit in the head by a thrown spear. Default is false.";
            var heavierSpearsLabel = new OpLabel(heavierSpearsBox.PosX + 40f, heavierSpearsBox.PosY, "Heavier spears");
            heavierSpearsLabel.description = heavierSpearsBox.description;
            */

            var heavierSpearsConfig = new QuConfigOption(new OpCheckBox(HeavierSpears, 16f, 440f),
                "Heavier spears",
                "Causes lizards to flip over upon being hit in the head by a thrown spear. Default is false.");
            
            opTab.AddItems(titleBox);
            opTab.AddItems(easierTongueBreakConfig.Elements);
            opTab.AddItems(lessTriggerHappyDropwigsConfig.Elements);
            opTab.AddItems(heavierSpearsConfig.Elements);
        }

        private class QuConfigOption
        {
            public UIelement[] Elements { get; }

            public QuConfigOption(UIconfig config, string name, string desc)
            {
                config.description = desc;
                var label = new OpLabel(config.PosX + config.size.x + 20f, config.PosY, name)
                {
                    description = desc
                };
                Elements = new UIelement[] { config, label };
            }
        }
    }
}