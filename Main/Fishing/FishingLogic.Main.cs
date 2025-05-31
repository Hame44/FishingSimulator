public void CastLine():
{
}

public void PullLine():
{
}

public void Hook():
{
    if (isFishBiting) {
        isHooked = true;
        isFishBiting = false;
    }
    else {
        // TODO: Wait long for the next bite
        isFishBiting = false;
    }
}