using TMPro;

public class StageSelectButton : UIKeyButton
{
    public int stageNum = 1;
    public TextMeshProUGUI textMesh;
    private StageInfoBase stageInfo;

    public void SetStageInfo(StageInfoBase stageInfoBase)
    {
        stageInfo = stageInfoBase;
        stageNum = stageInfoBase.stage;
        textMesh.text = "Stage " + stageInfo.stage + " " + stageInfo.difficulty;
    }
}