select l.name as lang, ts.DatasetSize as Size, tt.Name as timeType, MeanAverage as Time, ts.RecordCount  from stats.TimeStatistics ts
	join TimeTypes tt on tt.timetypeid = ts.timetypeid 
	join Languages l on ts.LanguageId = l.LanguageId
	where tt.TimeTypeId = 8
	order by lang, timeType, size
	
	use AppStats

	select * from Environments

	update stats.TimeStatistics set meanaverage = 573
	where EnvironmentId = 1 and ProcessorCount = 12
	and languageid =2 and DatasetSize = 1000000