<?php
/**
 * This file is only included if profiling is enabled
 * @package MediaWiki
 */

/**
 * @param $functioname name of the function we will profile
 */
function wfProfileIn( $functionname ) {
	global $wgProfiler;
	$wgProfiler->profileIn( $functionname );
}

/**
 * @param $functioname name of the function we have profiled
 */
function wfProfileOut( $functionname = 'missing' ) {
	global $wgProfiler;
	$wgProfiler->profileOut( $functionname );
}

function wfGetProfilingOutput() {
	global $wgProfiler;
	return $wgProfiler->getOutput();
}

function wfProfileClose() {
	global $wgProfiler;
	$wgProfiler->close();
}

if( !function_exists( 'memory_get_usage' ) ) {
	# Old PHP or --enable-memory-limit not compiled in
	function memory_get_usage() {
		return 0;
	}
}

/**
 * @todo document
 * @package MediaWiki
 */
class Profiler
{
	var $mStack = array(), $mWorkStack = array(), $mCollated = array();
	var $mCalls = array(), $mTotals = array();
	/*
	function Profiler()
	{
		$this->mProfileStack = array();
		$this->mWorkStack = array();
		$this->mCollated = array();
	}
	*/
	
	function profileIn( $functionname ) {
		global $wgDebugFunctionEntry;
		if ( $wgDebugFunctionEntry && function_exists( 'wfDebug' ) ) {
			wfDebug( str_repeat( ' ', count( $this->mWorkStack ) ) . 'Entering '.$functionname."\n" );
		}
		$this->mWorkStack[] = array($functionname, count( $this->mWorkStack ), microtime(), memory_get_usage() );
	}

	function profileOut( $functionname ) {
		$memory = memory_get_usage();
		$time = microtime();
		global $wgDebugProfiling, $wgDebugFunctionEntry;

		if ( $wgDebugFunctionEntry && function_exists( 'wfDebug' ) ) {
			wfDebug( str_repeat( ' ', count( $this->mWorkStack ) ) . 'Exiting '.$functionname."\n" );
		}
		
		$bit = array_pop( $this->mWorkStack );
		
		if ( !$bit ) {
			wfDebug( "Profiling error, !\$bit: $functionname\n" );
		} else {
			if ( $wgDebugProfiling ) {
				if ( $functionname == 'close' ) {
					wfDebug( "Profile section ended by close(): {$bit[0]}\n" );
				} elseif ( $bit[0] != $functionname ) {
					wfDebug( "Profiling error: in({$bit[0]}), out($functionname)\n" );
				}
			}
			$bit[] = $time;
			$bit[] = $memory;
			$this->mStack[] = $bit;
		}
	}
	
	function close() {
		while ( count( $this->mWorkStack ) ) {
			$this->profileOut( 'close' );
		}
	}

	function getOutput() {
		global $wgDebugFunctionEntry;
		$wgDebugFunctionEntry = false;

		if( !count( $this->mStack ) ) {
			return "No profiling output\n";
		}
		$this->close();
		
		global $wgProfileCallTree;
		if( $wgProfileCallTree ) {
			return $this->getCallTree();
		} else {
			return $this->getFunctionReport();
		}
	}
	
	function getCallTree( $start = 0 ) {
		return implode( '',
			array_map(
				array( &$this, 'getCallTreeLine' ),
				$this->remapCallTree( $this->mStack ) ) );
	}
	
	function remapCallTree( $stack ) {
		if( count( $stack ) < 2 ) {
			return $stack;
		}
		$outputs = array();
		for( $max = count( $stack ) - 1; $max > 0; ) {
			/* Find all items under this entry */
			$level = $stack[$max][1];
			$working = array();
			for( $i = $max - 1; $i >= 0; $i-- ) {
				if( $stack[$i][1] > $level ) {
					$working[] = $stack[$i];
				} else {
					break;
				}
			}
			$working = $this->remapCallTree( array_reverse( $working ) );
			$output = array();
			foreach( $working as $item ) {
				array_push( $output, $item );
			}
			array_unshift( $output, $stack[$max] );
			$max = $i;
			
			array_unshift( $outputs, $output );
		}
		$final = array();
		foreach( $outputs as $output ) {
			foreach( $output as $item ) {
				$final[] = $item;
			}
		}
		return $final;
	}
	
	function getCallTreeLine( $entry ) {
		list( $fname, $level, $start, $x, $end ) = $entry;
		$delta = $this->microDelta( $start, $end );
		$space = str_repeat( ' ', $level );
		
		# The ugly double sprintf is to work around a PHP bug,
		# which has been fixed in recent releases.
		return sprintf( "%10s %s %s\n",
			trim( sprintf( "%7.3f", $delta * 1000.0 ) ),
			$space, $fname );
	}
	
	function micro2Float( $micro ) {
		list( $whole, $fractional ) = explode( ' ', $micro );
		return (float)$whole + (float)$fractional;
	}
	
	function microDelta( $start, $end ) {
		return $this->micro2Float( $end ) -
		       $this->micro2Float( $start );
	}
	
	function getFunctionReport() {		
		$width = 140;
		$nameWidth = $width - 65;
		$format =      "%-{$nameWidth}s %6d %13.3f %13.3f %13.3f%% %9d  (%13.3f -%13.3f) [%d]\n";
		$titleFormat = "%-{$nameWidth}s %6s %13s %13s %13s %9s %14s   %14s %9s\n";
		$prof = "\nProfiling data\n";
		$prof .= sprintf( $titleFormat, 'Name', 'Calls', 'Total', 'Each', '%', 
			'Mem', 'Min', 'Max', 'Overhead' );
		$this->mCollated = array();
		$this->mCalls = array();
		$this->mMemory = array();
		
		# Estimate profiling overhead
		$profileCount = count( $this->mStack );
		wfProfileIn( '-overhead-total' );
		for ($i=0; $i<$profileCount ; $i++) {
			wfProfileIn( '-overhead-internal' );
			wfProfileOut( '-overhead-internal' );
		}
		wfProfileOut( '-overhead-total' );
		
		# First, subtract the overhead!
		foreach( $this->mStack as $entry ) {
			$fname = $entry[0];
			$thislevel = $entry[1];
			$start = explode( ' ', $entry[2]);
			$start = (float)$start[0] + (float)$start[1];
			$end = explode( ' ', $entry[4]);
			$end = (float)$end[0] + (float)$end[1];
			$elapsed = $end - $start;
			$memory = $entry[5] - $entry[3];
			
			if( $fname == '-overhead-total' ) {
				$overheadTotal[] = $elapsed;
				$overheadMemory[] = $memory;
			} elseif( $fname == '-overhead-internal' ) {
				$overheadInternal[] = $elapsed;
			}
		}
		$overheadTotal = array_sum( $overheadTotal ) / count( $overheadInternal );
		$overheadMemory = array_sum( $overheadMemory ) / count( $overheadInternal );
		$overheadInternal = array_sum( $overheadInternal ) / count( $overheadInternal );
		
		# Collate
		foreach ( $this->mStack as $index => $entry ) {
			$fname = $entry[0];
			$thislevel = $entry[1];
			$start = explode( ' ', $entry[2]);
			$start = (float)$start[0] + (float)$start[1];
			$end = explode( ' ', $entry[4]);
			$end = (float)$end[0] + (float)$end[1];
			$elapsed = $end - $start;
			
			$memory = $entry[5] - $entry[3];
			$subcalls = $this->calltreeCount( $this->mStack, $index );
			
			if( !preg_match( '/^-overhead/', $fname ) ) {
				# Adjust for profiling overhead
				$elapsed -= $overheadInternal;
				$elapsed -= ($subcalls * $overheadTotal);
				$memory -= ($subcalls * $overheadMemory);
			}
			
			if ( !array_key_exists( $fname, $this->mCollated ) ) {
				$this->mCollated[$fname] = 0;
				$this->mCalls[$fname] = 0;
				$this->mMemory[$fname] = 0;
				$this->mMin[$fname] = 1 << 24;
				$this->mMax[$fname] = 0;
				$this->mOverhead[$fname] = 0;
			}

			$this->mCollated[$fname] += $elapsed;
			$this->mCalls[$fname] ++;
			$this->mMemory[$fname] += $memory;
			$this->mMin[$fname] = min( $this->mMin[$fname], $elapsed );
			$this->mMax[$fname] = max( $this->mMax[$fname], $elapsed );
			$this->mOverhead[$fname] += $subcalls;
		}

		$total = @$this->mCollated['-total'];
		$this->mCalls['-overhead-total'] = $profileCount;

		# Output
		asort( $this->mCollated, SORT_NUMERIC );
		foreach ( $this->mCollated as $fname => $elapsed ) {
			$calls = $this->mCalls[$fname];
			$percent = $total ? 100. * $elapsed / $total : 0;
			$memory = $this->mMemory[$fname];
			$prof .= sprintf( $format, substr( $fname, 0, $nameWidth), $calls, (float)($elapsed * 1000), 
					(float)($elapsed * 1000) / $calls, $percent, $memory,
					($this->mMin[$fname] * 1000.0),
					($this->mMax[$fname] * 1000.0),
					$this->mOverhead[$fname] );

			global $wgProfileToDatabase;
		}
		$prof .= "\nTotal: $total\n\n";

		return $prof;
	}

	/**
	 * Counts the number of profiled function calls sitting under
	 * the given point in the call graph. Not the most efficient algo.
	 *
	 * @param array $stack
	 * @param int $start
	 * @return int
	 * @access private
	 */
	function calltreeCount( &$stack, $start ) {
		$level = $stack[$start][1];
		$count = 0;
		for( $i = $start - 1; $i >= 0 && $stack[$i][1] > $level; $i-- ) {
			$count++;
		}
		return $count;
	}
}


$wgProfiler = new Profiler();
$wgProfiler->profileIn( '-total' );
?>
