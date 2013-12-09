select * from stats.TimeStatistics where ProcessorCount=0

select count(*) from stats.TimeStatistics
SELECT DISTINCT timetypeid, processorcount, datasetsize, environmentid, languageid, count(*) cnt from stats.TimeStatistics group by TimeTypeId, ProcessorCount, DatasetSize, EnvironmentId, LanguageId having count(*)>1
SELECT DISTINCT * from stats.TimeStatistics order by RecordCount desc
select * from Records 
	join Times t on Records.RecordId=t.RecordId where ProcessorCount=1 and languageid=1 and EnvironmentId=1 and t.TimeTypeId=2

delete stats.TimeStatistics

update times set [time]=[time]/1000 where timeid in(
select timeid from [times] join Records on Records.RecordId= Times.RecordId
where Records.LanguageId=1)

--delete Stats.TimeStatistics where batchtime is null
select * from Languages

select distinct * from Records

select distinct batchtime, RecordCount, TimeStatisticid from stats.TimeStatistics where TimeStatisticid>30646 order by 1 desc

select * from stats.TimeStatistics
select distinct RecordCount from stats.TimeStatistics

select distinct count(*), AVG(RecordCount) from Stats.TimeStatistics 

select distinct t1.*,t2.* from (select distinct processorcount, environmentid, languageid, timetypeid, datasetsize from stats.TimeStatistics where TimeStatisticId<30646) t1
					full OUTER JOIN 
						(select  distinct processorcount, environmentid, languageid, timetypeid, datasetsize from stats.TimeStatistics where TimeStatisticId>=30646) t2
					on t2.ProcessorCount = t1.ProcessorCount and t2.EnvironmentId= t1.EnvironmentId and t2.LanguageId = t1.LanguageId and t2.TimeTypeId = t1.TimeTypeId and t1.DatasetSize=t2.DatasetSize
					


;WITH CTE AS (SELECT ROW_NUMBER() over (order by timestatisticid) AS RowNo, batchtime FROM stats.TimeStatistics where batchtime>'2013-09-29 22:54:54.3976706')

--;WITH CTE AS (SELECT ROW_NUMBER() over (order by timestatisticid) AS RowNo, batchtime FROM stats.TimeStatistics where timestatisticid<30646)
select * from (SELECT distinct t1.batchtime bt1, t2.batchtime bt2, ISNULL(DATEDIFF(SECOND, t2.batchtime, t1.batchtime), 0) AS Mins
FROM CTE t1
    LEFT JOIN CTE t2 ON t1.RowNo = t2.RowNo + 1
) t3 where t3.mins>0 order by bt1 desc

select DATEDIFF(SECOND,t1.batchTime, t2.batchTime) from 
	(select top 1 batchtime from Stats.TimeStatistics) t1,
	(select top 1 batchtime from stats.TimeStatistics order by 1 desc) t2
	
	select * from DropFiles
select distinct dropfileid from Records
select * from records where environmentid=2 and languageid=2
select * from Environments
select * from languages


select * from DropFiles


select distinct batchtime, RecordCount  from stats.TimeStatistics order by batchtime

select * from stats.TimeStatistics where batchtime>='2013-09-29 21:29:18.9619405'

--delete stats.TimeStatistics where batchtime>='2013-09-29 21:29:18.9619405'

select * from Records

select count(*) from records 
	join times t on records.RecordId= t.RecordId group by ProcessorCount, languageid, Size, EnvironmentId, timetypeid order by Size, ProcessorCount

delete from Times
delete  from Records
delete from DropFiles
delete from DropFileStore

select * from DropFiles

select count(*) from records 
	where dropfileid in (select top 1 dropfileid from dropfiles order by dropfileid desc) 

	select * from records where DropFileId=175

select * from DropFiles

select count(*) from records where dropfileid = 168

select count(Size), Size from Records group by Size


select * from records where ProcessorCount = 256 and LanguageId=1

select * from stats.TimeStatistics 
where EnvironmentId=1 and LanguageId=2 and ProcessorCount=12 and DatasetSize=1000000

select * from Records r left join times t on r.RecordId=t.TimeId

select * from Records r left join Times t on t.RecordId = r.RecordId

where r.LanguageId=2 and r.ProcessorCount=12 and r.EnvironmentId=1 and Size= 70000000

--delete records 
select count(*)
from Records left join times on Records.recordid = times.RecordId
where times.RecordId is null