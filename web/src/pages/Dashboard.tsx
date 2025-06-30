import React, { useState, useEffect, useMemo } from 'react';
import {
  Box,
  Typography,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Fab,
  Snackbar,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  MenuItem,
  TextField
} from '@mui/material';
import { Edit as EditIcon, Refresh as RefreshIcon, Send as SendIcon } from '@mui/icons-material';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getAssignments, updateAssignment, triggerRotationUpdate, getMembers, sendToSlack } from '../services/api';
import { TaskAssignment, ModifyAssignment, Member } from '../types';
import { format, parseISO } from 'date-fns';

const Dashboard: React.FC = () => {
  const [selectedAssignment, setSelectedAssignment] = useState<TaskAssignment | null>(null);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedMember, setSelectedMember] = useState({
    host: '',
    startDate: '',
    endDate: '',
  });
  const [isLoading, setIsLoading] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { data: assignments } = useQuery<TaskAssignment[]>({
    queryKey: ['assignments'],
    queryFn: getAssignments,
  });

  const { data: members } = useQuery<Member[]>({
    queryKey: ['members'],
    queryFn: getMembers,
  });

  const queryClient = useQueryClient();

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
    mutationFn: (variables: { id: number; data: ModifyAssignment }) =>
      updateAssignment(variables.id, variables.data),
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

  const handleEditClick = (assignment: TaskAssignment) => {
    setSelectedAssignment(assignment);
    setSelectedMember({
      host: assignment.host,
      startDate: assignment.startDate,
      endDate: assignment.endDate,
    });
    setEditDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setEditDialogOpen(false);
    setSelectedAssignment(null);
    setSelectedMember({
      host: '',
      startDate: '',
      endDate: '',
    });
  };

  const handleSave = async () => {
    if (!selectedAssignment) return;

    const data: ModifyAssignment = {
      host: selectedMember.host,
      startDate: selectedMember.startDate,
      endDate: selectedMember.endDate,
    };

    await updateAssignmentMutation.mutateAsync({
      id: selectedAssignment.id,
      data,
    });
  };

  const handleUpdateRotation = async () => {
    await updateRotationMutation.mutateAsync();
  };

  const handleSendToSlack = async () => {
    setIsLoading(true);
    setError(null);
    try {
      await sendToSlack();
      setShowSuccess(true);
    } catch (err) {
      setError('Failed to send message to Slack');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Box p={3}>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">
          Dashboard
        </Typography>
        <Button
          variant="contained"
          startIcon={<RefreshIcon />}
          onClick={handleUpdateRotation}
          disabled={updateRotationMutation.isPending}
        >
          Update Rotation
        </Button>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Task</TableCell>
              <TableCell>Assignee</TableCell>
              <TableCell>Start Date</TableCell>
              <TableCell>End Date</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {currentAssignments.map((assignment) => (
              <TableRow key={assignment.id}>
                <TableCell>{assignment.taskName}</TableCell>
                <TableCell>{assignment.host}</TableCell>
                <TableCell>{format(parseISO(assignment.startDate), 'yyyy-MM-dd')}</TableCell>
                <TableCell>{format(parseISO(assignment.endDate), 'yyyy-MM-dd')}</TableCell>
                <TableCell>
                  <Button
                    startIcon={<EditIcon />}
                    onClick={() => handleEditClick(assignment)}
                  >
                    Edit
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={editDialogOpen} onClose={handleCloseDialog}>
        <DialogTitle>Edit Assignment</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <TextField
              select
              fullWidth
              label="Assignee"
              value={selectedMember.host}
              onChange={(e) => setSelectedMember({ ...selectedMember, host: e.target.value })}
              sx={{ mb: 2 }}
            >
              {members?.map((member) => (
                <MenuItem key={member.id} value={member.host}>
                  {member.host}
                </MenuItem>
              ))}
            </TextField>
            <TextField
              fullWidth
              label="Start Date"
              type="date"
              value={selectedMember.startDate}
              onChange={(e) => setSelectedMember({ ...selectedMember, startDate: e.target.value })}
              InputLabelProps={{ shrink: true }}
              sx={{ mb: 2 }}
            />
            <TextField
              fullWidth
              label="End Date"
              type="date"
              value={selectedMember.endDate}
              onChange={(e) => setSelectedMember({ ...selectedMember, endDate: e.target.value })}
              InputLabelProps={{ shrink: true }}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button
            onClick={handleSave}
            variant="contained"
            disabled={updateAssignmentMutation.isPending}
          >
            Save
          </Button>
        </DialogActions>
      </Dialog>

      <Box position="fixed" bottom={16} right={16}>
        <Fab
          color="primary"
          onClick={handleSendToSlack}
          disabled={isLoading}
        >
          <SendIcon />
        </Fab>
      </Box>

      <Snackbar
        open={showSuccess}
        autoHideDuration={3000}
        onClose={() => setShowSuccess(false)}
      >
        <Alert severity="success">
          Message sent to Slack successfully
        </Alert>
      </Snackbar>

      <Snackbar
        open={!!error}
        autoHideDuration={3000}
        onClose={() => setError(null)}
      >
        <Alert severity="error">
          {error}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default Dashboard; 