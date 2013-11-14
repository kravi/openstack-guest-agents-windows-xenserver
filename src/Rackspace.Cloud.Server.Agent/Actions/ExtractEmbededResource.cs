// Copyright 2011 OpenStack LLC.
// All Rights Reserved.
//
//    Licensed under the Apache License, Version 2.0 (the "License"); you may
//    not use this file except in compliance with the License. You may obtain
//    a copy of the License at
//
//         http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
//    WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
//    License for the specific language governing permissions and limitations
//    under the License.


using System.IO;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface IExtractEmbededResource
    {
        void Extract(string outputDir, string resourceLocation, string fileName);
    }

    public class ExtractEmbededResource : IExtractEmbededResource
    {
        public void Extract(string outputDir, string resourceLocation, string fileName)
        {
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLocation))
            {
                using (System.IO.FileStream fileStream = new System.IO.FileStream(System.IO.Path.Combine(outputDir, fileName), System.IO.FileMode.Create))
                {
                    for (int i = 0; i < stream.Length; i++)
                    {
                        fileStream.WriteByte((byte)stream.ReadByte());
                    }
                    fileStream.Close();
                }
            }
        }
    }
}
