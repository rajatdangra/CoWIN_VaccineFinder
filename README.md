# Co-WIN: Vaccine Finder
Vaccine Finder: based on Pincode and Age Limit Criteria.
Send notification on Mail, if slots are available.
Book Slots and Send notification on Mail, if Booked successfully.


Installation instructions:
	1) Unzip folder.
	2) Install Setup.
	3) Fill in the basic details.
Note: Details once filled, will be retained to use for next time. It will be picked automatically. You can also check VaccineFinder.exe.config for more details.


User Inputs:

Mandatory:
	1) Phone number
	2) Pin-Codes : Search is based on Priority order defined (first come basis). Format - Comma separated (eg. 100001, 100002)
	3) Beneficiary Ids: Ids found in Co-Win app against individual user. Format - Comma separated (eg. 123456, 7891011)
	4) Dose (eg. 1 or 2)
	5) Vaccine (eg. ANY, COVISHIELD, COVAXIN, SPUTNIK V)
	6) Email Ids: Email ids where mail will be sent in case of slots availability. Format - Comma separated (eg. abc@gmail.com, def@gmail.com)
	7) Minimum Age Limit: 18 or 45
	8) Slot Preference: (1=> 09:00AM-11:00AM, 2=> 11:00AM-01:00PM, 3=> 01:00PM-03:00PM, 4=> After 03:00PM)
	9) From Date: Date from which vaccination slots will be searched (for 7 days). Format - "dd-MM-yyyy" (eg. 31-03-2021)
	10) Retry Frequency (in Seconds): Frequency at which retry call should happen (in case of no slots found), (eg. 10). Value less than 3 Seconds is not recommended due to rate limiting imposed by the Government.
	11) AutoPickCenter - If True ("1"), it auto picks center with the combination of 'Pin-code priority and the center with most available slots'. If false, it asks user to input preferred center.
	12) IncludePaidService - If True ("1"), it will also include centers with as 'Paid' Service (Fee Type). If false, it will only pick centers with 'Free' service.

Optional:
	1) First Name - will be used in E-Mail.
	2) Last Name - will be used in E-Mail.
	
In App.Config: (Highly Recommended not to update these settings)
	1) VerifyBeneficiaries - To verify beneficiaries, before slot booking (highly recommended: True)
	2) SendEmail - True or False to enable mail.

Process:
	1) Confirm inputs from user.
	2) Generate OTP.
	3) Verify OTP.
	4) Verify Beneficiaries.
	5) Check Available slots.
	Case 1) Vaccine slots available: Program will Display Available Centres, make a beep sound to notify, and Users will get E-Mails on defined Email Ids. Proceed to Booking (next step).			
	Case 2) Vaccine slots not available: Program will retry at user defined Retry Frequency.
	6) Start Booking:
			1) AutoBookCenter:
				case a) If AutoBookCenter is False ("0"), get preferred center input (number) from user, and only try to book slots of specific center. 
				case b) If AutoBookCenter is True ("1"), it will sort available center based on availability, then tries to book slot, it will hit all the centers until slot is successfully booked.
			2) Try to book slots, with Slot preference, then book other slots (whichever is available).
			3) Confirmation mail on User email ids.
	7) Automatically close in 15 seconds.
	
	
Note: This is a Personal Project, I do NOT endorse this for Public use. Developed for Educational Purpose only; USE AT YOUR OWN RISK. I SHOULD NOT BE DEEMED RESPONSIBLE FOR ANY LEGAL CONCERNS. DO NOT SHARE THIS.