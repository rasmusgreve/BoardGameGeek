package itu.ejuu.neural;

import itu.ejuu.PacManUtilities;
import pacman.Executor;
import pacman.controllers.Controller;
import pacman.controllers.examples.Legacy2TheReckoning;
import pacman.controllers.examples.StarterGhosts;
import pacman.game.Constants.MOVE;
import pacman.game.Game;

public class NNAgent extends Controller<MOVE> {

	public static int RESTARTS = 0;
	
	public static int LIMIT = 1000; //Integer.MAX_VALUE;
	public static boolean DEBUG = false;
	
	public static int STATE_VALUES = 19;
	
	public static void main(String[] args){
		Executor exec = new Executor();
		System.out.println("NeuralNetwork vs StarterGhosts");
		NNAgent nn = new NNAgent();
		exec.runExperiment(nn, new StarterGhosts(), 25);
		System.out.println("NeuralNetwork vs Legacy2");
		exec.runExperiment(nn, new Legacy2TheReckoning(), 25);
	}
	
	private static double[][] parseOutput(DataTuple[] data) {
		double[][] result = new double[data.length][];
		
		for(int i = 0; i < data.length; i++){
			result[i] = parseOutput(data[i]);
		}
		return result;
	}
	
	private static double[] parseOutput(DataTuple data) {
		double[] result = new double[4];
		result[PacManUtilities.getMove(data.DirectionChosen)] = 1.0;
		return result;
	}

	private static double[][] parseInput(DataTuple[] data) {
		double[][] result = new double[data.length][];
		
		for(int i = 0; i < data.length; i++){
			result[i] = parseInput(data[i]);
		}
		return result;
	}
	
	private static double[] parseInput(DataTuple data){
		double[] result = new double[STATE_VALUES];
		result[0] = data.mazeIndex;
		result[1] = data.normalizeLevel(data.currentLevel);
		result[2] = data.normalizePosition(data.pacmanPosition);
		result[3] = data.pacmanLivesLeft/3.0;
		result[4] = data.normalizeCurrentScore(data.currentScore);
		result[5] = data.normalizeTotalGameTime(data.totalGameTime);
		result[6] = data.normalizeCurrentLevelTime(data.currentLevelTime);
		result[7] = data.normalizeNumberOfPills(data.numOfPillsLeft);
		result[8] = data.normalizeNumberOfPowerPills(data.numOfPowerPillsLeft);
		result[9] = data.normalizeBoolean(data.isBlinkyEdible);
		result[10]= data.normalizeBoolean(data.isInkyEdible);
		result[11]= data.normalizeBoolean(data.isPinkyEdible);
		result[12]= data.normalizeBoolean(data.isSueEdible);
		result[13]= PacManUtilities.getMove(data.blinkyDir)/4.0;
		result[14]= PacManUtilities.getMove(data.inkyDir)/4.0;
		result[15]= PacManUtilities.getMove(data.pinkyDir)/4.0;
		result[16]= PacManUtilities.getMove(data.sueDir)/4.0;
		result[17]= PacManUtilities.getMove(data.pillDir)/4.0;
		result[18]= PacManUtilities.getMove(data.powerPillDir)/4.0;
		return result;
	}

	private NeuralNetwork nn;
	
	public NNAgent(){
		nn = new NeuralNetwork(STATE_VALUES,4,STATE_VALUES/2,2);
		runTraining();
	}

	private void runTraining() {
		DataTuple[] data = DataSaverLoader.LoadPacManData();
		double[][] input = parseInput(data);
		double[][] output = parseOutput(data);
		int i = 0;
		int correct = 0;
		
		while(correct < input.length && i < LIMIT){ // stopping condition				
			if(DEBUG) System.out.println("----- Running Training "+i+" L="+Node.L+" -----");
			correct = nn.runTraining(input,output);
			System.out.println(correct);
			i++;
			
			if(DEBUG && i % 1000 == 0) System.out.println(nn.toString());
			
			// update L
			Node.L = 1 - correct / ((double)input.length);
		}
		
		if(correct == input.length){
			System.out.println("Complete after "+i+" runs.");
		}
		System.out.println(nn.toString());
	}
	
	private MOVE getLargestMove(double[] output){
		double maxValue = output[0];
		int maxIndex = 0;
		for(int i = 1; i < output.length; i++){
			if(output[i] > maxValue){
				maxValue = output[i];
				maxIndex = i;
			}
		}
		if(DEBUG && maxValue < 0.5) System.out.println("NN: Weak output!");
		return PacManUtilities.getMove(maxIndex);
	}

	@Override
	public MOVE getMove(Game game, long timeDue) {
		double[] result = nn.getOutput(parseInput(new DataTuple(game,MOVE.NEUTRAL)));
		return getLargestMove(result);
	}

}
