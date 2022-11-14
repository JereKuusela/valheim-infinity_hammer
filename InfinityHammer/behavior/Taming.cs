namespace InfinityHammer;
public static class Taming
{
  public static void SetTame(Character obj, bool tame)
  {
    if (!obj) return;
    obj.SetTamed(tame);
    var AI = obj.GetComponent<BaseAI>();
    if (AI)
    {
      AI.SetAlerted(false);
      if (tame)
      {
        SetHunt(AI, false);
        AI.SetPatrolPoint();
      }
      AI.SetTargetInfo(ZDOID.None);
      var monster = obj.GetComponent<MonsterAI>();
      if (monster)
      {
        monster.m_targetCreature = null;
        monster.m_targetStatic = null;
        if (tame)
        {
          monster.SetDespawnInDay(false);
          monster.SetEventCreature(false);
        }
      }
      var animal = obj.GetComponent<AnimalAI>();
      if (animal)
      {
        animal.m_target = null;
      }
    }
  }
  public static void SetHunt(BaseAI obj, bool hunt)
  {
    if (!obj) return;
    obj.SetHuntPlayer(hunt);
  }
}
