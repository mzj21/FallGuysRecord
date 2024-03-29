﻿using System.Collections.Generic;

public class RoundCompletedEpisodeDto
{
    public int Kudos { get; set; }
    public int Fame { get; set; }
    public int Crowns { get; set; }
    public int CurrentCrownShards { get; set; }
    public List<Round> ListRound { get; set; }

    public class Round
    {
        public int RoundNum { get; set; }
        public string RoundName { get; set; }
        public string RoundShowName { get; set; }
        public bool Qualified { get; set; }
        public int Position { get; set; }
        public int Kudos { get; set; }
        public int Fame { get; set; }
        public int BonusTier { get; set; }
        public int BonusKudos { get; set; }
        public int BonusFame { get; set; }
        public string BadgeId { get; set; }

        public Round()
        {
            RoundNum = 0;
            RoundName = "";
            RoundShowName = "";
            Qualified = false;
            Position = 0;
            Kudos = 0;
            Fame = 0;
            BonusTier = 0;
            BonusKudos = 0;
            BonusFame = 0;
            BadgeId = "";
        }

        public override string ToString()
        {
            return $"{{{nameof(RoundNum)}={RoundNum.ToString()}, {nameof(RoundName)}={RoundName}, {nameof(RoundShowName)}={RoundShowName}, {nameof(Qualified)}={Qualified.ToString()}, {nameof(Position)}={Position.ToString()}, {nameof(Kudos)}={Kudos.ToString()}, {nameof(Fame)}={Fame.ToString()}, {nameof(BonusTier)}={BonusTier.ToString()}, {nameof(BonusKudos)}={BonusKudos.ToString()}, {nameof(BonusFame)}={BonusFame.ToString()}, {nameof(BadgeId)}={BadgeId}}}";
        }
    }

    public RoundCompletedEpisodeDto()
    {
        Kudos = 0;
        Fame = 0;
        Crowns = 0;
        CurrentCrownShards = 0;
        ListRound = new List<Round>();
    }

    public override string ToString()
    {
        return $"{{{nameof(Kudos)}={Kudos.ToString()}, {nameof(Fame)}={Fame.ToString()}, {nameof(Crowns)}={Crowns.ToString()}, {nameof(CurrentCrownShards)}={CurrentCrownShards.ToString()}, {nameof(ListRound)}={string.Join(",", ListRound)}}}";
    }
}