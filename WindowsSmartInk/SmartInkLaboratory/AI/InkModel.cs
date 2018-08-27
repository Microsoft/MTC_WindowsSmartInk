using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;

// 4b7e8535-9fcf-4162-aae1-af3f171e249b_1115394e-f1eb-4398-a55f-14ade85ae44e

namespace Microsoft.MTC.SmartInk
{
    public sealed class ModelInput
    {
        public VideoFrame data { get; set; }
    }

    public sealed class ModelOutput
    {
        public IList<string> classLabel { get; set; }
        public IDictionary<string, float> loss { get; set; }
        public ModelOutput(Dictionary<string, float> tags)
        {
            this.classLabel = new List<string>();
            this.loss = tags;
        }
    }

    public sealed class InkModel
    {
        private LearningModelPreview learningModel;
        private Dictionary<string, float> _tags;

        public InkModel(Dictionary<string,float> tags)
        {
            if (tags == null || tags.Keys.Count == 0)
                throw new InvalidOperationException($"{nameof(tags)} cannot be null or empty.");
            _tags = tags;
        }

 

        public static async Task<InkModel> CreateModelAsync(IStorageFile file, IList<string> tags)
        {
            var tagValues = new Dictionary<string, float>();
            foreach (var t in tags)
            {
                tagValues.Add(t, float.NaN);
            }

            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            InkModel model = new InkModel(tagValues);
            model.learningModel = learningModel;
            return model;
        }
        public async Task<ModelOutput> EvaluateAsync(ModelInput input) {
            ModelOutput output = new ModelOutput(_tags);

            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("loss", output.loss);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
