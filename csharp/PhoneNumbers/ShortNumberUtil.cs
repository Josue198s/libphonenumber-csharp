﻿/*
* Copyright (C) 2011 The Libphonenumber Authors
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhoneNumbers
{
    /*
    * Utility for short phone numbers, such as short codes and emergency numbers.
    *
    * @author Shaopeng Jia
    */
    public class ShortNumberUtil
    {
        private static PhoneNumberUtil phoneUtil;

        public ShortNumberUtil()
        {
            phoneUtil = PhoneNumberUtil.GetInstance();
        }

        // @VisibleForTesting
        public ShortNumberUtil(PhoneNumberUtil util)
        {
            phoneUtil = util;
        }

        /**
        * Returns true if the number might be used to connect to an emergency service in the given
        * region.
        *
        * This method takes into account cases where the number might contain formatting, or might have
        * additional digits appended (when it is okay to do that in the region specified).
        *
        * @param number  the phone number to test
        * @param regionCode  the region where the phone number is being dialed
        * @return  if the number might be used to connect to an emergency service in the given region.
        */
        public bool ConnectsToEmergencyNumber(String number, String regionCode)
        {
            return MatchesEmergencyNumberHelper(number, regionCode, true /* allows prefix match */);
        }

        /**
        * Returns true if the number exactly matches an emergency service number in the given region.
        *
        * This method takes into account cases where the number might contain formatting, but doesn't
        * allow additional digits to be appended.
        *
        * @param number  the phone number to test
        * @param regionCode  the region where the phone number is being dialed
        * @return  if the number exactly matches an emergency services number in the given region.
        */
        public bool IsEmergencyNumber(String number, String regionCode)
        {
            return MatchesEmergencyNumberHelper(number, regionCode, false /* doesn't allow prefix match */);
        }

        private bool MatchesEmergencyNumberHelper(String number, String regionCode,
            bool allowPrefixMatch)
        {
            number = PhoneNumberUtil.ExtractPossibleNumber(number);
            if (PhoneNumberUtil.PLUS_CHARS_PATTERN.MatchBeginning(number).Success)
            {
                // Returns false if the number starts with a plus sign. We don't believe dialing the country
                // code before emergency numbers (e.g. +1911) works, but later, if that proves to work, we can
                // add additional logic here to handle it.
                return false;
            }
            PhoneMetadata metadata = phoneUtil.GetMetadataForRegion(regionCode);
            if (metadata == null || !metadata.HasEmergency)
            {
                return false;
            }
            var emergencyNumberPattern =
                new PhoneRegex(metadata.Emergency.NationalNumberPattern);
            String normalizedNumber = PhoneNumberUtil.NormalizeDigitsOnly(number);
            // In Brazil, it is impossible to append additional digits to an emergency number to dial the
            // number.
            return (!allowPrefixMatch || regionCode.Equals("BR"))
                ? emergencyNumberPattern.MatchAll(normalizedNumber).Success
                : emergencyNumberPattern.MatchBeginning(normalizedNumber).Success;

        }

        /**
        * Returns true if the number matches the short code number format for the given region.
        *
        * This method takes into account cases where the number might contain formatting, but doesn't
        * allow additional digits to be appended.
        *
        * @param number  the phone number to test
        * @param regionCode  the region where the phone number is being dialed
        * @return  if the number matches the short code number format for the given region.
        */
        public bool IsShortcodeNumber(string number, string regionCode) {

            var phoneMetadataForRegion = phoneUtil.GetMetadataForRegion(regionCode);
            if (phoneMetadataForRegion == null || !phoneMetadataForRegion.HasShortCode) {
                // NOTE: We should also probably do this when phoneMetadataForRegion.ShortCode.NationalNumberPattern.Equals("NA")
                // I think there is a bug where PhoneNumbers.BuildMetadataFromXml.LoadGeneralDesc always calls metadata.SetShortCode(),
                // which always sets HasShortCode to true, even in the cases where PhoneNumbers.BuildMetadataFromXml.ProcessPhoneNumberDescElement 
                // returns "NA" (which happens when the territory in PhoneNumberMetaData.xml does not contain a shortCode definition/node)
                return false;
            }

            var shortCodeNumberPattern = new PhoneRegex(phoneMetadataForRegion.ShortCode.NationalNumberPattern);
            var normalizedNumber = PhoneNumberUtil.NormalizeDigitsOnly(number);

            return shortCodeNumberPattern.MatchAll(normalizedNumber).Success;
        }
    }
}
