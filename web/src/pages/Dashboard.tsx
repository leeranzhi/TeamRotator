import React, { useState, useMemo } from 'react';
import {
  Box,
  Typography,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Stack,
  Tooltip,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
} from '@mui/material';
import { Edit as EditIcon, Refresh as RefreshIcon } from '@mui/icons-material';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getAssignments, updateAssignment, triggerRotationUpdate, getMembers } from '../services/api';
import { TaskAssignment, ModifyAssignment, Member } from '../types';
import { format, parseISO } from 'date-fns';

const Dashboard: React.FC = () => {
  const queryClient = useQueryClient();
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedAssignment, setSelectedAssignment] = useState<TaskAssignment | null>(null);
  const [editForm, setEditForm] = useState({
    host: '',
    startDate: '',
    endDate: '',
  });

  const { data: assignments } = useQuery<TaskAssignment[]>({
    queryKey: ['assignments'],
    queryFn: getAssignments,
  });

  const { data: members } = useQuery<Member[]>({
    queryKey: ['members'],
    queryFn: getMembers,
  });

  // 获取每个任务的最新分配记录
  const currentAssignments = useMemo(() => {
    if (!assignments) return [];
    
    // 使用Map来保存每个taskId对应的最新分配记录
    const latestAssignments = new Map<number, TaskAssignment>();
    
    assignments.forEach(assignment => {
      const existing = latestAssignments.get(assignment.taskId);
      // 如果这个任务还没有记录，或者当前记录的ID比已存在的大（更新），则更新Map
      if (!existing || assignment.id > existing.id) {
        latestAssignments.set(assignment.taskId, assignment);
      }
    });
    
    // 将Map转换为数组
    return Array.from(latestAssignments.values());
  }, [assignments]);

  const updateAssignmentMutation = useMutation({
    mutationFn: ({ id, assignment }: { id: number; assignment: ModifyAssignment }) =>
      updateAssignment(id, assignment),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['assignments'] });
      handleCloseDialog();
    },
  });

  const updateRotationMutation = useMutation({
    mutationFn: triggerRotationUpdate,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['assignments'] });
    },
  });

  const formatDate = (dateString: string | null | undefined) => {
    if (!dateString) return '';
    try {
      if (/^\d{4}-\d{2}-\d{2}$/.test(dateString)) {
        return dateString;
      }
      return format(parseISO(dateString), 'yyyy-MM-dd');
    } catch (error) {
      console.error('Error formatting date:', error, dateString);
      return '';
    }
  };

  const handleEdit = (assignment: TaskAssignment) => {
    setSelectedAssignment(assignment);
    setEditForm({
      host: assignment.host || '',
      startDate: formatDate(assignment.startDate) || '',
      endDate: formatDate(assignment.endDate) || '',
    });
    setEditDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setEditDialogOpen(false);
    setSelectedAssignment(null);
    setEditForm({
      host: '',
      startDate: '',
      endDate: '',
    });
  };

  const handleSave = async () => {
    if (!selectedAssignment) return;
    
    await updateAssignmentMutation.mutateAsync({
      id: selectedAssignment.id,
      assignment: {
        host: editForm.host,
        startDate: editForm.startDate,
        endDate: editForm.endDate,
      },
    });
  };

  const handleUpdateRotation = async () => {
    await updateRotationMutation.mutateAsync();
  };

  return (
    <Box p={3}>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">Current Task Assignments</Typography>
        <Tooltip title="Update rotation">
          <IconButton onClick={handleUpdateRotation} color="primary">
            <RefreshIcon />
          </IconButton>
        </Tooltip>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Task Name</TableCell>
              <TableCell>Assigned To</TableCell>
              <TableCell>Start Date</TableCell>
              <TableCell>End Date</TableCell>
              <TableCell>Slack ID</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {currentAssignments.map((assignment) => (
              <TableRow key={assignment.id}>
                <TableCell>
                  <Typography variant="body1">{assignment.taskName}</Typography>
                </TableCell>
                <TableCell>
                  <Typography variant="body1">{assignment.host}</Typography>
                </TableCell>
                <TableCell>
                  <Typography variant="body1">
                    {formatDate(assignment.startDate)}
                  </Typography>
                </TableCell>
                <TableCell>
                  <Typography variant="body1">
                    {formatDate(assignment.endDate)}
                  </Typography>
                </TableCell>
                <TableCell>
                  <Chip
                    label={assignment.slackId}
                    size="small"
                    color="primary"
                    variant="outlined"
                  />
                </TableCell>
                <TableCell>
                  <IconButton onClick={() => handleEdit(assignment)} size="small">
                    <EditIcon />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
            {(!currentAssignments || currentAssignments.length === 0) && (
              <TableRow>
                <TableCell colSpan={6} align="center">
                  <Typography variant="body2" color="text.secondary">
                    No assignments found
                  </Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={editDialogOpen} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>Edit Assignment</DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ mt: 2 }}>
            <FormControl fullWidth>
              <InputLabel>Member</InputLabel>
              <Select
                value={editForm.host}
                onChange={(e) => setEditForm({ ...editForm, host: e.target.value })}
                label="Member"
              >
                {members?.map((member) => (
                  <MenuItem key={member.id} value={member.host}>
                    {member.host}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <TextField
              label="Start Date"
              type="date"
              value={editForm.startDate}
              onChange={(e) => setEditForm({ ...editForm, startDate: e.target.value })}
              fullWidth
              InputLabelProps={{
                shrink: true,
              }}
            />
            <TextField
              label="End Date"
              type="date"
              value={editForm.endDate}
              onChange={(e) => setEditForm({ ...editForm, endDate: e.target.value })}
              fullWidth
              InputLabelProps={{
                shrink: true,
              }}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSave} variant="contained" color="primary">
            Save
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default Dashboard; 