# Co-WIN: Vaccine Finder
Vaccine Finder: based on Pincode and Age Limit Criteria.
Send notification on Mail, if slots are available.


Installation instructions:
	1) Unzip folder.
	2) Install Setup.
	3) Fill in the basic details.
Note: Details once filled, will be retained to use for next time. It will be picked automatically.


User Inputs:

Mandatory:
	1) Pin-Code
	2) Email Ids: Email ids where mail will be sent in case of slots availability. Format - Comma separated (eg. abc@gmail.com, def@gmail.com)
	3) Minimum Age Limit: 18 or 45
	4) From Date: Date from which vaccination slots will be searched (for 7 days). Format - "dd-MM-yyyy" (eg. 31-03-2021)
	5) Retry Frequency (in Seconds): Frequency at which retry call should happen (in case of no slots found), (eg. 10). Value less than 3 Seconds is not recommended due to rate limiting imposed by the Government.

Optional:
	1) First Name - will be used in E-Mail.
	2) Last Name - will be used in E-Mail.
	
	
Process:
	Case 1) Vaccine slots available: Program will make a beep sound to notify, and Users will get E-Mails on defined Email Ids.
	Case 2) Vaccine slots not available: Program will retry at user defined Retry Frequency.