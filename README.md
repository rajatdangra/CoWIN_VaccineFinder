# Co-WIN: Vaccine Finder
Vaccine Finder: based on Pincode and Age Limit Criteria.
Send notification on Mail, if slots are available.


Installation instructions:
	1) Unzip folder.
	2) Install Setup.
	3) Fill in the basic details.
Note: Details once filled, will be retained to use for next time. It will be picked automatically. You can also check VaccineFinder.exe.config for more details.


User Inputs:

Mandatory:
	1) Phone number
	2) Beneficiary Ids: Ids found in Co-Win app against individual user. Format - Comma separated (eg. 123456, 7891011)
	3) Dose (eg. 1 or 2)
	4) Pin-Code
	5) Email Ids: Email ids where mail will be sent in case of slots availability. Format - Comma separated (eg. abc@gmail.com, def@gmail.com)
	6) Minimum Age Limit: 18 or 45
	7) Slot Preference: (1=> 09:00AM-11:00AM, 2=> 11:00AM-01:00PM, 3=> 01:00PM-03:00PM, 4=> After 03:00PM)
	8) From Date: Date from which vaccination slots will be searched (for 7 days). Format - "dd-MM-yyyy" (eg. 31-03-2021)
	9) Retry Frequency (in Seconds): Frequency at which retry call should happen (in case of no slots found), (eg. 10). Value less than 3 Seconds is not recommended due to rate limiting imposed by the Government.

Optional:
	1) First Name - will be used in E-Mail.
	2) Last Name - will be used in E-Mail.
	
In App.Config:
	1) AutoBookCenter - If True ("1"), it auto picks center with the most available slots. If false, it asks user to input preferred center.
	2) VerifyBeneficiaries - To verify beneficiaries, before slot booking (highly recommended: True)
	3) SendEmail - True or False to enable mail.

Process:
	1) Confirm inputs from user.
	2) Generate OTP.
	3) Verify OTP.
	4) Verify Beneficiaries.
	5) Check Available slots.
	Case 1) Vaccine slots available: Program will make a beep sound to notify, and Users will get E-Mails on defined Email Ids.
			1) Take preferred center input from user if AutoBookCenter.
			2) Try to book slots, with Slot preference, then book other slots (whichever is available).
	Case 2) Vaccine slots not available: Program will retry at user defined Retry Frequency.