package itu.ejuu.neural;

import itu.ejuu.PacManUtilities;
import pacman.game.Constants;
import pacman.game.Constants.DM;
import pacman.game.Constants.GHOST;
import pacman.game.Constants.MOVE;
import pacman.game.Game;

public class DataTuple {
	
	public MOVE DirectionChosen;
	
	//General game state this - not normalized!
	public int mazeIndex;
	public int currentLevel;
	public int pacmanPosition;
	public int pacmanLivesLeft;
	public int currentScore;
	public int totalGameTime;
	public int currentLevelTime;
	public int numOfPillsLeft;
	public int numOfPowerPillsLeft;
	
	//Ghost this, dir, dist, edible - BLINKY, INKY, PINKY, SUE
	public boolean isBlinkyEdible = false;
	public boolean isInkyEdible = false;
	public boolean isPinkyEdible = false;
	public boolean isSueEdible = false;
	
	public int blinkyDist = -1;
	public int inkyDist = -1;
	public int pinkyDist = -1;
	public int sueDist = -1;
	
	public MOVE blinkyDir;
	public MOVE inkyDir;
	public MOVE pinkyDir;
	public MOVE sueDir;
	
	public MOVE pillDir;
	public MOVE powerPillDir;
	
	//Util data - useful for normalization
	public int numberOfNodesInLevel;
	public int numberOfTotalPillsInLevel;
	public int numberOfTotalPowerPillsInLevel;
	
	public DataTuple(Game game, MOVE move)
	{
		if(move == MOVE.NEUTRAL)
		{
			move = game.getPacmanLastMoveMade();
		}
		
		this.DirectionChosen = move;
		
		this.mazeIndex = game.getMazeIndex();
		this.currentLevel = game.getCurrentLevel();
		this.pacmanPosition = game.getPacmanCurrentNodeIndex();
		this.pacmanLivesLeft = game.getPacmanNumberOfLivesRemaining();
		this.currentScore = game.getScore();
		this.totalGameTime = game.getTotalTime();
		this.currentLevelTime = game.getCurrentLevelTime();
		this.numOfPillsLeft = game.getNumberOfActivePills();
		this.numOfPowerPillsLeft = game.getNumberOfActivePowerPills();
		
		if (game.getGhostLairTime(GHOST.BLINKY) == 0) {
			this.isBlinkyEdible = game.isGhostEdible(GHOST.BLINKY);
			this.blinkyDist = game.getShortestPathDistance(game.getPacmanCurrentNodeIndex(),game.getGhostCurrentNodeIndex(GHOST.BLINKY));
		}
		
		if (game.getGhostLairTime(GHOST.INKY) == 0) {
		this.isInkyEdible = game.isGhostEdible(GHOST.INKY);
		this.inkyDist = game.getShortestPathDistance(game.getPacmanCurrentNodeIndex(),game.getGhostCurrentNodeIndex(GHOST.INKY));
		}
		
		if (game.getGhostLairTime(GHOST.PINKY) == 0) {
		this.isPinkyEdible = game.isGhostEdible(GHOST.PINKY);
		this.pinkyDist = game.getShortestPathDistance(game.getPacmanCurrentNodeIndex(),game.getGhostCurrentNodeIndex(GHOST.PINKY));
		}
		
		if (game.getGhostLairTime(GHOST.SUE) == 0) {
		this.isSueEdible = game.isGhostEdible(GHOST.SUE);
		this.sueDist = game.getShortestPathDistance(game.getPacmanCurrentNodeIndex(),game.getGhostCurrentNodeIndex(GHOST.SUE));
		}
		
		this.blinkyDir = game.getNextMoveTowardsTarget(game.getPacmanCurrentNodeIndex(),game.getGhostCurrentNodeIndex(GHOST.BLINKY), DM.PATH);
		this.inkyDir = game.getNextMoveTowardsTarget(game.getPacmanCurrentNodeIndex(),game.getGhostCurrentNodeIndex(GHOST.INKY), DM.PATH);
		this.pinkyDir = game.getNextMoveTowardsTarget(game.getPacmanCurrentNodeIndex(),game.getGhostCurrentNodeIndex(GHOST.PINKY), DM.PATH);
		this.sueDir = game.getNextMoveTowardsTarget(game.getPacmanCurrentNodeIndex(),game.getGhostCurrentNodeIndex(GHOST.SUE), DM.PATH);
		
		this.pillDir = PacManUtilities.getPacMoveToClosestPill(game);
		this.powerPillDir = PacManUtilities.getPacMoveToClosestPower(game);
		
		this.numberOfNodesInLevel = game.getNumberOfNodes();
		this.numberOfTotalPillsInLevel = game.getNumberOfPills();
		this.numberOfTotalPowerPillsInLevel = game.getNumberOfPowerPills();
	}
	
	public DataTuple(String data)
	{
		String[] dataSplit = data.split(";");
		
		this.DirectionChosen = MOVE.valueOf(dataSplit[0]);
		
		this.mazeIndex = Integer.parseInt(dataSplit[1]);
		this.currentLevel = Integer.parseInt(dataSplit[2]);
		this.pacmanPosition = Integer.parseInt(dataSplit[3]);
		this.pacmanLivesLeft = Integer.parseInt(dataSplit[4]);
		this.currentScore = Integer.parseInt(dataSplit[5]);
		this.totalGameTime = Integer.parseInt(dataSplit[6]);
		this.currentLevelTime = Integer.parseInt(dataSplit[7]);
		this.numOfPillsLeft = Integer.parseInt(dataSplit[8]);
		this.numOfPowerPillsLeft = Integer.parseInt(dataSplit[9]);
		this.isBlinkyEdible = Boolean.parseBoolean(dataSplit[10]);
		this.isInkyEdible = Boolean.parseBoolean(dataSplit[11]);
		this.isPinkyEdible = Boolean.parseBoolean(dataSplit[12]);
		this.isSueEdible = Boolean.parseBoolean(dataSplit[13]);
		this.blinkyDist = Integer.parseInt(dataSplit[14]);
		this.inkyDist = Integer.parseInt(dataSplit[15]);
		this.pinkyDist = Integer.parseInt(dataSplit[16]);
		this.sueDist = Integer.parseInt(dataSplit[17]);
		this.blinkyDir = MOVE.valueOf(dataSplit[18]);
		this.inkyDir = MOVE.valueOf(dataSplit[19]);
		this.pinkyDir = MOVE.valueOf(dataSplit[20]);
		this.sueDir = MOVE.valueOf(dataSplit[21]);
		this.pillDir = MOVE.valueOf(dataSplit[22]);
		this.powerPillDir = MOVE.valueOf(dataSplit[23]);
		this.numberOfNodesInLevel = Integer.parseInt(dataSplit[24]);
		this.numberOfTotalPillsInLevel = Integer.parseInt(dataSplit[25]);
		this.numberOfTotalPowerPillsInLevel = Integer.parseInt(dataSplit[26]);
	}
	
	public String getSaveString()
	{
		StringBuilder stringbuilder = new StringBuilder();
		
		stringbuilder.append(this.DirectionChosen+";");
		stringbuilder.append(this.mazeIndex+";");
		stringbuilder.append(this.currentLevel+";");
		stringbuilder.append(this.pacmanPosition+";");
		stringbuilder.append(this.pacmanLivesLeft+";");
		stringbuilder.append(this.currentScore+";");
		stringbuilder.append(this.totalGameTime+";");
		stringbuilder.append(this.currentLevelTime+";");
		stringbuilder.append(this.numOfPillsLeft+";");
		stringbuilder.append(this.numOfPowerPillsLeft+";");
		stringbuilder.append(this.isBlinkyEdible+";");
		stringbuilder.append(this.isInkyEdible+";");
		stringbuilder.append(this.isPinkyEdible+";");
		stringbuilder.append(this.isSueEdible+";");
		stringbuilder.append(this.blinkyDist+";");
		stringbuilder.append(this.inkyDist+";");
		stringbuilder.append(this.pinkyDist+";");
		stringbuilder.append(this.sueDist+";");
		stringbuilder.append(this.blinkyDir+";");
		stringbuilder.append(this.inkyDir+";");
		stringbuilder.append(this.pinkyDir+";");
		stringbuilder.append(this.sueDir+";");
		stringbuilder.append(this.pillDir+";");
		stringbuilder.append(this.powerPillDir+";");
		stringbuilder.append(this.numberOfNodesInLevel+";");
		stringbuilder.append(this.numberOfTotalPillsInLevel+";");
		stringbuilder.append(this.numberOfTotalPowerPillsInLevel+";");
		
		return stringbuilder.toString();
	}

	/**
	 * Used to normalize distances. Done via min-max normalization.
	 * Supposes that minimum possible distance is 0. Supposes that
	 * the maximum possible distance is the total number of nodes in
	 * the current level.
	 * @param dist Distance to be normalized
	 * @return Normalized distance
	 */
	public double normalizeDistance(int dist)
	{
		return ((dist-0)/(this.numberOfNodesInLevel-0))*(1-0)+0;
	}
	
	public double normalizeLevel(int level)
	{
		return ((level-0)/(Constants.NUM_MAZES-0))*(1-0)+0;
	}
	
	public double normalizePosition(int position)
	{
		return ((position-0)/(this.numberOfNodesInLevel-0))*(1-0)+0;
	}
	
	public double normalizeBoolean(boolean bool)
	{
		if(bool)
		{
			return 1.0;
		}
		else
		{
			return 0.0;
		}
	}
	
	public double normalizeNumberOfPills(int numOfPills)
	{
		return ((numOfPills-0)/(this.numberOfTotalPillsInLevel-0))*(1-0)+0;
	}
	
	public double normalizeNumberOfPowerPills(int numOfPowerPills)
	{
		return ((numOfPowerPills-0)/(this.numberOfTotalPowerPillsInLevel-0))*(1-0)+0;
	}
	
	public double normalizeTotalGameTime(int time)
	{
		return ((time-0)/(Constants.MAX_TIME-0))*(1-0)+0;
	}
	
	public double normalizeCurrentLevelTime(int time)
	{
		return ((time-0)/(Constants.LEVEL_LIMIT-0))*(1-0)+0;
	}
	
	/**
	 * 
	 * Max score value lifted from highest ranking PacMan controller on PacMan vs Ghosts
	 * website: http://pacman-vs-ghosts.net/controllers/1104
	 * @param score
	 * @return
	 */
	public double normalizeCurrentScore(int score)
	{
		return ((score-0)/(82180-0))*(1-0)+0;
	}
	
}
