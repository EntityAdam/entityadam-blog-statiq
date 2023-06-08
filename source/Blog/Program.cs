return await Bootstrapper
  .Factory
  .CreateWeb(args)
  .SetOutputPath("bin/Release/output")
  .RunAsync();