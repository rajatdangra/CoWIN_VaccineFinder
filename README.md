# Co-WIN: _Vaccine Finder_
<ins>Vaccine Finder</ins>: based on Pincode and Age Limit Criteria.
Send notification on Mail, if slots are available.
Book Slots and Send notification on Mail and Telegram, if Booked successfully.

_Platforms Supported_: Windows, macOS, Linux.

<ins>_Installation instructions_</ins>:
1. Unzip folder.
2. Run VaccineFinder.exe
3. Fill in the basic details.
_Note_: Details once filled, will be retained to use for next time. It will be picked automatically. You can also check appsettings.json for more details.


<ins>***User Inputs***</ins>:

_Mandatory_:
1. Phone number
2. Pin-Codes : Search is based on Priority order defined (first come basis). Format - Comma separated (eg. 100001, 100002)
3. Beneficiary Ids: Ids found in Co-Win app against individual user. Format - Comma separated (eg. 123456, 7891011)
4. Dose (eg. 1 or 2)
5. Vaccine (eg. ANY, COVISHIELD, COVAXIN, SPUTNIK V)
6. Email Ids: Email ids where mail will be sent in case of slots availability. Format - Comma separated (eg. abc@gmail.com, def@gmail.com)
7. Minimum Age Limit: 18 or 45
8. Slot Preference: (1=> 09:00AM-11:00AM, 2=> 11:00AM-01:00PM, 3=> 01:00PM-03:00PM, 4=> After 03:00PM)
9. From Date: Date from which vaccination slots will be searched (for 7 days). Format - "dd-MM-yyyy" (eg. 31-03-2021)
10. Retry Frequency (in Seconds): Frequency at which retry call should happen (in case of no slots found), (eg. 10). Value less than 3 Seconds is not recommended due to rate limiting imposed by the Government.
11. AutoPickCenter - If True ("1"), it auto picks center with the combination of 'Pin-code priority and the center with most available slots'. If false, it asks user to input preferred center.
12. IncludePaidService - If True ("1"), it will also include centers with as 'Paid' Service (Fee Type). If false, it will only pick centers with 'Free' service.
13. Telegram Chat ID: Chat ID with @Covid19VaccineSlotFinderBot, will not send Notification if this field is left Empty in appsettings.json. You can Get your Chat ID by Pinging @Covid19VaccineSlotFinderBot, and then we can update this on the fly based on UserName.

_Optional_:
1. First Name - will be used in E-Mail.
2. Last Name - will be used in E-Mail.
	
_In **appsettings.json**_: (Highly Recommended not to update these settings)
1. VerifyBeneficiaries - To verify beneficiaries, before slot booking (highly recommended: True)
2. SendEmail - True or False to enable/disable Mail.
3. SendNotification - True or False to enable/disable Telegram Notifications.
4. AutomaticCloseProgramWaitTime (in Seconds)- At the point of termination, program will wait for seconds before closing Automatically.
5. TryToBookOtherSlots - True or False. Try to Book Slots other than Slot Preference specified in UserPreference if enabled ("1"), By default disabled ("0") as we think availability is for the entire day, it is not segregated in individual slots

<ins>***Process***</ins>:
1. Check Latest Version of the Application.
2. Confirm inputs from user.
3. Generate OTP.
4. Verify OTP.
5. Verify Beneficiaries. Verify Vaccine and Dose specified, Update valid Vaccine and Dose in settings automatically.
6. Check Available slots.
Case 1. Vaccine slots available: Program will Display Available Centres, make a beep sound to notify, and Users will get E-Mails on defined Email Ids. Proceed to Booking (next step).			
Case 2. Vaccine slots not available: Program will retry at user defined Retry Frequency. Automatically re-generate OTP, if session is expired.
7. Start Booking:
   - AutoBookCenter:
		case a) If AutoBookCenter is False ("0"), get preferred center input (number) from user, and only try to book slots of specific center. 
		case b) If AutoBookCenter is True ("1"), it will sort available center based on availability, then tries to book slot, it will hit all the centers until slot is successfully booked.
   - Try to book slots, with Slot preference, then book other slots (depending on TryToBookOtherSlots) whichever is available.
   - Confirmation mail on User email ids.
8. Automatically close in 30 seconds (depends on AutomaticCloseProgramWaitTime setting in appsettings.json).


<ins>***Note***</ins>: _This is a Personal Project, I do NOT endorse this for Public use. Developed for Educational Purpose only; USE AT YOUR OWN RISK. I SHOULD NOT BE DEEMED RESPONSIBLE FOR ANY LEGAL CONCERNS. DO NOT SHARE THIS_.