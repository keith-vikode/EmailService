@ECHO OFF
SetLocal
Set ConnectionStrings:SqlServer=Server=(local); Database=EmailService; Integrated Security=true; MultipleActiveResultSets=True
Set ConnectionStrings:Storage=UseDevelopmentStorage=true
CALL run EmailService.Processor.Job